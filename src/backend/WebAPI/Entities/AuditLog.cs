using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Entities;

[Table("audit_log")]
public class AuditLog : Entity
{
    [Column("at", TypeName = "timestamp with time zone")]
    public DateTime At { get; set; }

    [Required]
    [Column("made_by")]
    public required string MadeBy { get; set; }

    [Column("record_owner_id")]
    public Guid? RecordOwnerId { get; set; }

    [Column("channel")]
    [StringLength(16)]
    public string? Channel { get; set; }

    [Column("action")]
    [StringLength(40)]
    public string? Action { get; set; }

    [Column("entity_type")]
    [StringLength(100)]
    public string? EntityType { get; set; }

    [Column("entity_id")]
    public Guid? EntityId { get; set; }

    [Column("changes", TypeName = "jsonb")]
    public AuditLogChanges? Changes { get; set; }

    [Column("metadata")]
    public string? Metadata { get; set; }

    [Column("correlation_id")]
    public Guid CorrelationId { get; set; }
}
