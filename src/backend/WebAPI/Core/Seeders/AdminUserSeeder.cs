using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using WebAPI.Configuration.Options;
using WebAPI.Core.Services;
using WebAPI.Core.Utils.Interfaces;
using WebAPI.Data;
using WebAPI.Data.Types;

namespace WebAPI.Core.Seeders;

/// <summary>Cria o administrador da seção "Admin" quando o banco ainda não tem usuários.</summary>
public class AdminUserSeeder(IOptions<AdminUserOptions> options, IPasswordHasher<User> passwordHasher) : IDataSeeder
{
    public int Order => 0;

    public async Task SeedAsync(WebAPIContext context, CancellationToken cancellationToken = default)
    {
        AdminUserOptions admin = options.Value;

        if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Password))
            return;

        if (await context.Set<User>().IgnoreQueryFilters().AnyAsync(cancellationToken))
            return;

        User user = new()
        {
            Name = string.IsNullOrWhiteSpace(admin.Name) ? "Administrador" : admin.Name,
            Email = AuthService.NormalizeEmail(admin.Email),
            Role = UserRole.Admin,
        };
        user.PasswordHash = passwordHasher.HashPassword(user, admin.Password);

        context.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        Log.Information("[AdminUserSeeder] Usuário administrador {Email} criado.", user.Email);
    }
}
