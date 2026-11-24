using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Database;

public abstract class Entity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id", TypeName = "uuid")]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Column("created_at", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [AuditIgnore]
    [Column("updated_at", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("system_is_deleted", TypeName = "boolean")]
    public bool SystemIsDeleted { get; set; } = false;

    [Column("system_deleted_at", TypeName = "timestamp with time zone")]
    public DateTime? SystemDeletedAt { get; set; }
}
