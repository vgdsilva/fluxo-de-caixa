
namespace WebAPI.Utils.Builders;

public sealed class AuditLogBuilder
{
    private readonly AuditLog _auditLog;

    private readonly Dictionary<string, object?> _changes = new();

    private AuditLogBuilder()
    {
        _auditLog = new AuditLog
        {
            At = DateTime.UtcNow,
            MadeBy = string.Empty,
            CorrelationId = Guid.NewGuid()
        };
    }

    public static AuditLogBuilder Create()
        => new();

    public AuditLogBuilder WithAt(DateTime at)
    {
        _auditLog.At = at;
        return this;
    }

    public AuditLogBuilder WithMadeBy(string madeBy)
    {
        _auditLog.MadeBy = madeBy;
        return this;
    }

    public AuditLogBuilder WithRecordOwner(Guid? recordOwnerId)
    {
        _auditLog.RecordOwnerId = recordOwnerId;
        return this;
    }

    public AuditLogBuilder WithChannel(string channel)
    {
        _auditLog.Channel = channel;
        return this;
    }

    public AuditLogBuilder WithAction(string action)
    {
        _auditLog.Action = action;
        return this;
    }

    public AuditLogBuilder WithEntity(string entityType, Guid? entityId = null)
    {
        _auditLog.EntityType = entityType;
        _auditLog.EntityId = entityId;
        return this;
    }

    public AuditLogBuilder WithChanges(AuditLogChanges changes)
    {
        _auditLog.Changes = changes;
        return this;
    }

    public AuditLogBuilder AddChange(string property, object? oldValue, object? newValue)
    {
        _changes[property] = new
        {
            Old = oldValue,
            New = newValue
        };

        _auditLog.Changes = new AuditLogChanges(_changes);

        return this;
    }

    public AuditLogBuilder WithChanges(
        Dictionary<string, object?> properties)
    {
        _auditLog.Changes = new AuditLogChanges(properties);
        return this;
    }

    public AuditLogBuilder WithMetadata(string metadata)
    {
        _auditLog.Metadata = metadata;
        return this;
    }

    public AuditLogBuilder WithCorrelationId(Guid correlationId)
    {
        _auditLog.CorrelationId = correlationId;
        return this;
    }

    public AuditLog Build()
    {
        if (string.IsNullOrWhiteSpace(_auditLog.MadeBy))
            throw new InvalidOperationException(
                "MadeBy is required.");

        if (string.IsNullOrWhiteSpace(_auditLog.Action))
            throw new InvalidOperationException(
                "Action is required.");

        if (string.IsNullOrWhiteSpace(_auditLog.EntityType))
            throw new InvalidOperationException(
                "EntityType is required.");

        return _auditLog;
    }
}
