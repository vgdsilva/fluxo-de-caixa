using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using WebAPI.Core.Utils.Extensions;

namespace WebAPI.Core.Utils.Classes;

public class TypeUtils
{
    private static readonly ConcurrentDictionary<Type, Lazy<ReadOnlyCollection<PropertyInfo>>> GetProperties_Cache = new();

    /// <summary>
    /// Obtém as propriedades de um tipo com cache.
    /// </summary>
    /// <param name="type">O tipo a ser analisado.</param>
    /// <returns>Uma lista imutável de PropertyInfo.</returns>
    public static ReadOnlyCollection<PropertyInfo> GetProperties(Type type)
    {
        return GetProperties_Cache.GetOrAdd(type, t => new Lazy<ReadOnlyCollection<PropertyInfo>>(() =>
        {
            PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.AsReadOnly();
        })).Value;
    }

    /// <summary>
    /// Verifica se uma classe é assinada por outra.
    /// </summary>
    /// <param name="srcType">Tipo do objeto que se deseja verificar</param>
    /// <param name="checkedType">Tipo a ser conferir</param>
    public static bool IsAssignableFrom(Type srcType, Type checkedType) => checkedType.IsAssignableFrom(srcType);

    private static readonly ConcurrentDictionary<(string, string), Type?> CreateType_Cache = new();

    /// <summary>
    /// Cria um type a partir de um nome da classe
    /// por meio de busca de assembly no domínio da aplicação.
    /// </summary>
    /// <param name="assemblyname">Nome do assemblyname</param>
    /// <param name="name">Nome da classe</param>
    /// <returns>Type encontrado</returns>
    public static Type? CreateType(string assemblyname, string name)
    {
        if (CreateType_Cache.TryGetValue((assemblyname, name), out Type? type))
            return type;

        Assembly entitiesassembly = Assembly.Load(new AssemblyName(assemblyname));

        type = entitiesassembly
                   .GetTypes()
                   .FirstOrDefault(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant());

        CreateType_Cache[(assemblyname, name)] = type;

        return type;
    }

    /// <summary>
    /// Cria um type a partir de um nome da classe
    /// por meio de busca de assembly no domínio da aplicação.
    /// </summary>
    /// <param name="filter">Filtro de busca com a condição</param>
    /// <returns>Type encontrado</returns>
    public static Type? CreateType(string assemblyname, Func<Type, bool> filter)
    {
        Assembly entitiesassembly = Assembly.Load(new AssemblyName(assemblyname));

        Type? type = entitiesassembly
                   .GetTypes()
                   .FirstOrDefault(filter);

        return type;
    }

    /// <summary>
    /// Converte uma lista de string para lista de Type.
    /// </summary>
    /// <param name="types">lista com os nomes dos tipos.</param>
    /// <returns>Lista convertida</returns>
    public static List<Type> ConvertListStringToListType(List<string> types)
    {
        List<Type> convertedlist = [];

        foreach (string typename in types)
            if (CreateType("WebAPI", typename) is { } type)
                convertedlist.Add(type);

        return convertedlist;
    }

    /// <summary>
    /// Busca os tipos genericos parametrizados na super classe.
    /// </summary>
    /// <param name="classType">Classe a ser verificada</param>
    /// <returns>Tipos genericos assinados na super classe</returns>
    public static Type[] FindParametrizedSuperType(Type classType) => [.. classType.BaseType?.GenericTypeArguments ?? []];

    /// <summary>
    /// Busca os tipos genericos parametrizados em uma interface implementada.
    /// </summary>
    /// <param name="classType">Classe a ser verificada</param>
    /// <param name="interfaceType">Interface generica implementada</param>
    /// <returns>Tipos genericos assinados na super classe</returns>
    public static Type[] FindParametrizedInterfaceType(Type classType, Type interfaceType)
    {
        return classType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
            .SelectMany(i => i.GetGenericArguments())
            .ToArray();
    }

    private static readonly ConcurrentDictionary<(Type, Type[]), Type> CreateType_Cache2 = new();

    /// <summary>
    /// Cria um tipo genérico.
    /// </summary>
    /// <param name="baseType">Tipo genérico base</param>
    /// <param name="paramType">Assinatura de coringas de tipo</param>
    /// <returns>Tipo genérico criado</returns>
    public static Type CreateType(Type baseType, params Type[] paramType)
    {
        var key = (baseType, paramType);

        return CreateType_Cache2.GetOrAdd(key, k =>
        {
            Type genericType = k.Item1;
            Type parametrizedType = genericType.MakeGenericType(k.Item2);
            return parametrizedType;
        });
    }

    /// <summary>
    /// Cria uma instância de objeto por reflexão.
    /// </summary>
    /// <param name="type">Tipo a ser instanciado</param>
    /// <param name="args">Argumentos de construtor</param>
    /// <returns>Instância criada</returns>
    public static object? CreateInstance(Type type, params object?[] args)
    {
        object? instance = Activator.CreateInstance(type, args);
        return instance;
    }

    /// <summary>
    /// Verifica se o tipo é uma derivação de Enumerable.
    /// </summary>
    /// <param name="type">Tipo a ser verificado</param>
    public static bool IsEnumerableType(Type type) => IsInstanceOfGenericType(type, typeof(IEnumerable));

    /// <summary>
    /// Verifica se o tipo é uma derivação de Enumerable.
    /// </summary>
    /// <param name="value">instancia de objeto</param>
    public static bool IsEnumerableType(object? value) => value != null && IsEnumerableType(value.GetType());

    /// <summary>
    /// Verifica se o tipo informado é uma derivação de um tipo genérico.
    /// </summary>
    /// <param name="type">Tipo a ser verificado</param>
    /// <param name="genericType">Tipo genérico</param>
    /// <returns></returns>
    public static bool IsInstanceOfGenericType(Type? type, Type genericType)
    {
        while (type != null)
        {
            if (type.IsGenericType && genericType.IsAssignableFrom(type.GetGenericTypeDefinition()))
                return true;
            type = type.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Verifica se o tipo é um array de bytes.
    /// </summary>
    /// <param name="type">Tipo a ser verificado</param>
    /// <returns></returns>
    public static bool IsByteArray(Type type) => type.IsArray && typeof(byte).IsAssignableFrom(type.GetElementType());

    public static bool IsByteArray(object? value) => value != null && IsByteArray(value.GetType());

    public static bool IsValueType(Type type) => type.IsValueType || type.IsEnum || type.Equals(typeof(string));

    public static bool IsDateTimeOffSet(Type type)
    {
        return type.IsValueType
                        && (type.Equals(typeof(DateTimeOffset))
                         || type.Equals(typeof(DateTimeOffset?)));
    }

    public static bool IsDateTime(Type type)
    {
        return type.IsValueType
                        && (type.Equals(typeof(DateTime))
                         || type.Equals(typeof(DateTime?)));
    }

    public static bool IsDateTimeOrDateTimeOffset(Type type)
    {
        return type.IsValueType
                        && (type.Equals(typeof(DateTime))
                         || type.Equals(typeof(DateTime?))
                         || type.Equals(typeof(DateTimeOffset))
                         || type.Equals(typeof(DateTimeOffset?)));
    }

    /// <summary>
    /// Verifica se a tabela/entidade passada possui campos do tipo byte[].
    /// <summary>
    /// <param name="value">Tabela/entidade a ser verificada.</param>
    /// <returns>True se possui</returns>
    public static bool HasByteArrayFields(Type type)
    {
        List<PropertyInfo> byteArrayFields = GetByteArrayFields(type);
        return !byteArrayFields.IsNullOrEmpty();
    }

    private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> GetByteArrayFields_Cache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<PropertyInfo> GetByteArrayFields(Type type)
    {
        if (GetByteArrayFields_Cache.TryGetValue(type, out List<PropertyInfo>? byteArrayFields))
            return byteArrayFields;

        byteArrayFields = [];

        foreach (PropertyInfo propInfo in GetProperties(type))
            if (IsByteArray(propInfo.PropertyType))
                byteArrayFields.Add(propInfo);

        GetByteArrayFields_Cache.TryAdd(type, byteArrayFields);

        return byteArrayFields;
    }

    private static readonly ConcurrentDictionary<string, Type?> GetTypeByName_Cache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Type? GetTypeByName(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        return GetTypeByName_Cache.GetOrAdd(typeName, key =>
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => string.Equals(t.FullName, key, StringComparison.OrdinalIgnoreCase));
        });
    }

    private static readonly ConcurrentDictionary<string, Type?> GetTypeByTableName_Cache = new();

    public static Type? GetTypeByTableName(string tableName)
    {
        return GetTypeByTableName_Cache.GetOrAdd(tableName, name =>
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                Type? type = assembly.GetTypes()
                    .FirstOrDefault(t =>
                        ReflectionUtils.HasAnnotation<TableAttribute>(t) &&
                        ReflectionUtils.GetAttribute<TableAttribute>(t).Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (type != null)
                    return type;
            }

            foreach (Assembly assembly in assemblies)
            {
                Type? type = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (type != null)
                    return type;
            }

            return null;
        });
    }

}
