using System.Reflection;
using WebAPI.Common.Messaging;

namespace WebAPI.Framework.Messaging;

/// <summary>Mapeia o nome lógico gravado no envelope para o tipo .NET da mensagem.</summary>
public sealed class MessageTypeRegistry
{
    private readonly Dictionary<string, Type> _types = new(StringComparer.Ordinal);

    public void RegisterFromAssemblies(params Assembly[] assemblies)
    {
        foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
        {
            if (type is { IsAbstract: false, IsInterface: false } && typeof(IMessage).IsAssignableFrom(type))
            {
                _types[MessageTypeAttribute.GetName(type)] = type;
            }
        }
    }

    public bool TryResolve(string name, out Type type) => _types.TryGetValue(name, out type!);
}
