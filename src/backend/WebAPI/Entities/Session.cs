using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Entities;

/// <summary>
/// Sessão de login. O cliente guarda o refresh token em claro; aqui fica só o hash SHA-256 dele.
/// </summary>
[Audit(AuditOperations.Create | AuditOperations.Update)]
[Table("session")]
[Index(nameof(TokenHash), IsUnique = true)]
public class Session : Entity
{
    [AuditIgnore]
    [Required]
    [Column("token_hash")]
    [StringLength(64)]
    public required string TokenHash { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    [AuditIgnore]
    [Column("last_access_at", TypeName = "timestamp with time zone")]
    public DateTime LastAccessAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at", TypeName = "timestamp with time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("ip_address")]
    [StringLength(64)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [StringLength(300)]
    public string? UserAgent { get; set; }

    [Column("revoked_at", TypeName = "timestamp with time zone")]
    public DateTime? RevokedAt { get; set; }
}
