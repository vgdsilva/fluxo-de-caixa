namespace WebAPI.Data.Attributes;

/// <summary>
/// Operações que geram registro em <c>audit_log</c>.
/// </summary>
[Flags]
public enum AuditOperations
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 4,
    All = Create | Update | Delete
}

/// <summary>
/// Liga a auditoria da entidade para as operações informadas (padrão: todas).
/// Entidades sem este atributo não geram registro em <c>audit_log</c>, mas o soft delete continua valendo.
/// </summary>
/// <example><c>[Audit(AuditOperations.Create | AuditOperations.Delete)]</c></example>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class AuditAttribute(AuditOperations operations = AuditOperations.All) : Attribute
{
    public AuditOperations Operations { get; } = operations;
}
