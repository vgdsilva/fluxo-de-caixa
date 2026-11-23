using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebAPI.Core.Utils.Builders;
using WebAPI.Data.Attributes;
using WebAPI.Data.Types;

namespace WebAPI.Data.Interceptors;

/// <summary>
/// No SaveChanges: preenche datas, converte delete em soft delete e grava <see cref="AuditLog"/>
/// para entidades marcadas com <see cref="AuditAttribute"/>, ignorando propriedades com <see cref="AuditIgnoreAttribute"/>.
/// </summary>
/// <remarks>
/// O contexto usa NoTracking por padrão. Para o diff de um update sair certo, carregue a entidade com
/// <c>AsTracking()</c> e altere as propriedades; com <c>Update(entidade)</c> o EF não conhece os valores antigos.
/// </remarks>
public class AuditInterceptor(IHttpContextAccessor? httpContextAccessor = null) : SaveChangesInterceptor
{
    private const string SystemUser = "system";

    private static readonly ConcurrentDictionary<Type, AuditOperations> AuditedOperations = new();
    private static readonly ConcurrentDictionary<(Type, string), bool> IgnoredProperties = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context)
            ApplyAudit(context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
                                                                          InterceptionResult<int> result,
                                                                          CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
            ApplyAudit(context);

        return ValueTask.FromResult(result);
    }

    private void ApplyAudit(DbContext context)
    {
        context.ChangeTracker.DetectChanges();

        DateTime now = DateTime.UtcNow;
        (string madeBy, Guid? userId) = GetCurrentUser();

        var entries = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity is not AuditLog &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (EntityEntry<Entity> entry in entries)
        {
            EntityState originalState = entry.State;

            switch (originalState)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.SystemIsDeleted = true;
                    entry.Entity.SystemDeletedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;
            }

            if (!ShouldAudit(entry.Metadata.ClrType, originalState))
                continue;

            AuditLogBuilder builder = AuditLogBuilder
                .Create()
                .WithAt(now)
                .WithMadeBy(madeBy)
                .WithRecordOwner(userId)
                .WithChannel("API")
                .WithAction(originalState.ToString().ToLowerInvariant())
                .WithEntity(entry.Metadata.ClrType.Name, entry.Entity.Id);

            bool hasChanges = AddChanges(builder, entry, originalState);

            // update que só mexeu em propriedades ignoradas (ex.: last_access_at) não gera registro
            if (originalState == EntityState.Modified && !hasChanges)
                continue;

            context.Add(builder.Build());
        }
    }

    private (string MadeBy, Guid? UserId) GetCurrentUser()
    {
        ClaimsPrincipal? user = httpContextAccessor?.HttpContext?.User;
        string? sid = user?.FindFirstValue(ClaimTypes.Sid);

        return Guid.TryParse(sid, out Guid userId)
            ? (userId.ToString(), userId)
            : (SystemUser, null);
    }

    private static bool ShouldAudit(Type entityType, EntityState state)
    {
        AuditOperations operations = AuditedOperations.GetOrAdd(entityType,
            static type => type.GetCustomAttribute<AuditAttribute>(inherit: true)?.Operations ?? AuditOperations.None);

        AuditOperations operation = state switch
        {
            EntityState.Added => AuditOperations.Create,
            EntityState.Modified => AuditOperations.Update,
            EntityState.Deleted => AuditOperations.Delete,
            _ => AuditOperations.None
        };

        return operation != AuditOperations.None && operations.HasFlag(operation);
    }

    private static bool IsIgnored(Type entityType, PropertyEntry property)
    {
        return IgnoredProperties.GetOrAdd((entityType, property.Metadata.Name),
            static (_, info) => info?.GetCustomAttribute<AuditIgnoreAttribute>(inherit: true) is not null,
            property.Metadata.PropertyInfo);
    }

    private static bool AddChanges(AuditLogBuilder builder, EntityEntry<Entity> entry, EntityState state)
    {
        bool hasChanges = false;
        Type entityType = entry.Metadata.ClrType;

        foreach (PropertyEntry property in entry.Properties)
        {
            if (property.IsTemporary || IsIgnored(entityType, property))
                continue;

            string name = property.Metadata.Name;

            switch (state)
            {
                case EntityState.Added:
                    builder.AddChange(name, null, property.CurrentValue);
                    hasChanges = true;
                    break;

                case EntityState.Modified:
                    if (!property.IsModified || Equals(property.OriginalValue, property.CurrentValue))
                        continue;

                    builder.AddChange(name, property.OriginalValue, property.CurrentValue);
                    hasChanges = true;
                    break;

                case EntityState.Deleted:
                    builder.AddChange(name, property.OriginalValue, property.CurrentValue);
                    hasChanges = true;
                    break;
            }
        }

        return hasChanges;
    }
}
