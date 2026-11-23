using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebAPI.Data.Types;

namespace WebAPI.Data.Converters;

public class AuditLogChangesConverter : ValueConverter<AuditLogChanges?, string?>
{
    public AuditLogChangesConverter() : base(
        v => v == null ? null : v.ToJson(),
        v => v == null ? null : AuditLogChanges.FromJson(v))
    {
    }
}
