namespace WebAPI.Database.Attributes;

/// <summary>
/// A propriedade nunca entra nas alterações gravadas em <c>audit_log</c>
/// (ex.: hash de senha, hash de token, campos que mudam a cada acesso).
/// Um update que só altera propriedades ignoradas não gera registro.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class AuditIgnoreAttribute : Attribute;
