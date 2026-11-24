using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Entities;

[Audit]
[Table("user")]
[Index(nameof(Login), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User : Entity
{
    [Required]
    [Column("name")]
    [StringLength(120)]
    public required string Name { get; set; }

    /// <summary>Nome de acesso usado no login. Sempre gravado em minúsculas (ver User.Normalize).</summary>
    /// <summary>Login e e-mail são gravados e comparados assim: sem espaços nas pontas e em minúsculas.</summary>
    public static string Normalize(string value) => value.Trim().ToLowerInvariant();

    [Required]
    [Column("login")]
    [StringLength(60)]
    public required string Login { get; set; }

    /// <summary>Sempre gravado em minúsculas (ver User.Normalize).</summary>
    [Column("email")]
    [StringLength(254)]
    public required string Email { get; set; }

    [Column("role", TypeName = "integer")]
    public UserRole Role { get; set; } = UserRole.User;

    [AuditIgnore]
    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = "";

    [Column("enabled", TypeName = "boolean")]
    public bool IsEnabled { get; set; } = true;

    [Column("failed_login_count", TypeName = "integer")]
    public int FailedLoginCount { get; set; }

    [Column("locked_until", TypeName = "timestamp with time zone")]
    public DateTime? LockedUntil { get; set; }

    [Column("last_login_at", TypeName = "timestamp with time zone")]
    public DateTime? LastLoginAt { get; set; }
}
