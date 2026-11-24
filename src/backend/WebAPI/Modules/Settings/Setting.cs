using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Modules.Settings;

[Audit]
[Table("setting")]
public class Setting : Entity
{
    [Column("key")]
    [StringLength(255)]
    [Required]
    public required string Key { get; set; }

    [StringLength(8000)]
    [Column("value")]
    public string? Value { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User? UserEntity { get; set; }

    public override string ToString() => $"Key: {Key}, Value: {Value}, UserId: {UserId}";

    public static Setting Create(string key, string? value, Guid? userId = null) => new()
    {
        Key = key,
        Value = value,
        UserId = userId
    };
}
