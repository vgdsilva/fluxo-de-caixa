using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebAPI.Database.Mapping;

public class EntityMappingConfiguration(Type entityType)
{
    protected Type EntityType { get; } = entityType;

    public void Map(ModelBuilder modelBuilder)
    {
        var entityBuilder = modelBuilder.Entity(EntityType);
        IMutableEntityType entityMetadata = entityBuilder.Metadata;

        // Altera o nome da tabela para o nome da classe em caixa baixa
        string tableName = entityMetadata.GetTableName() ?? EntityType.Name;
        entityMetadata.SetTableName(tableName.ToLowerInvariant());

        // Altera o nome das colunas para o nome em caixa baixa
        foreach (IMutableProperty property in entityMetadata.GetProperties())
        {
            PropertyMap(property);
        }

        if (typeof(Entity).IsAssignableFrom(EntityType))
        {
            // e => e.SystemIsDeleted == false
            ParameterExpression e = Expression.Parameter(EntityType, "e");
            Expression ativo = Expression.Equal(
                Expression.Property(e, nameof(Entity.SystemIsDeleted)),
                Expression.Constant(false, typeof(bool)));

            entityBuilder.HasQueryFilter(Expression.Lambda(ativo, e));

            // índices únicos valem só entre os registros ativos
            foreach (IMutableIndex index in entityMetadata.GetIndexes().Where(i => i.IsUnique))
                index.SetFilter("system_is_deleted = false");
        }

        var cascadeFKs = entityMetadata.GetForeignKeys()
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    protected static void PropertyMap(IMutableProperty property)
    {
        string? columnName = property.GetColumnName();
        if (columnName is not null)
            property.SetColumnName(columnName.ToLowerInvariant());
    }
}
