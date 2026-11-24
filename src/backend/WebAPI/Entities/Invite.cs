using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Entities;

[Audit]
[Table("invite")]
[Index(nameof(TokenHash), IsUnique = true)]
public class Invite : Entity
{
    [AuditIgnore]
    [Column("token_hash")]
    [StringLength(64)]
    public required string TokenHash { get; set; }

    [Column("email")]
    [StringLength(254)]
    public required string Email { get; set; }

    [Column("expires_at", TypeName = "timestamp with time zone")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_by_user_id")]
    public Guid CreatedByUserId { get; set; }

    [Column("used_at", TypeName = "timestamp with time zone")]
    public DateTime? UsedAt { get; set; }

    [Column("used_by_user_id")]
    public Guid? UsedByUserId { get; set; }

    [Column("revoked_at", TypeName = "timestamp with time zone")]
    public DateTime? RevokedAt { get; set; }
}
