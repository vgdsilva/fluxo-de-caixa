namespace WebAPI.Common.Messaging;

/// <summary>
/// Define o nome lógico (estável) de uma mensagem. Sem o atributo, usa-se o nome da classe.
/// Use um nome estável para não quebrar mensagens já gravadas no outbox ao renomear a classe.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class MessageTypeAttribute(string name) : Attribute
{
    public string Name { get; } = name;

    public static string GetName(Type type) =>
        type.GetCustomAttributes(typeof(MessageTypeAttribute), false) is [MessageTypeAttribute attr, ..]
            ? attr.Name
            : type.Name;
}
