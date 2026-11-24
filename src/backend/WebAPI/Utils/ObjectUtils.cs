using System.Reflection;

namespace WebAPI.Utils;

public class ObjectUtils
{
    /// <summary>
    /// Verifica se o valor é igual ao valor default para o tipo de dados
    /// </summary>
    /// <param name="val">Valor a verificar</param>
    /// <typeparam name="T">Tipo</typeparam>
    public static bool IsDefault<T>(T val)
    {
        return EqualityComparer<T>.Default.Equals(val, default(T));
    }

    /// <summary>
    /// Verifica se o valor é igual ao valor default para o tipo de dados
    /// </summary>
    /// <param name="val">Valor a verificar</param>
    /// <param name="type">Tipo</param>
    public static bool IsDefault(object val,
                                 Type type)
    {
        return EqualityComparer<object>.Default.Equals(val, Activator.CreateInstance(type));
    }

    /// <summary>
    /// Verifica se o valor é nulo ou igual ao valor default para o tipo de dados
    /// </summary>
    /// <param name="value">Valor a verificar</param>
    public static bool IsNullOrDefault(object? value)
    {
        if (Object.ReferenceEquals(value, null))
            return true;

        var type = value.GetType();
        return type.IsValueType
            && Object.Equals(value, Activator.CreateInstance(type));
    }

    /// <summary>
    /// Verifica se o valor é nulo ou vazio
    /// </summary>
    /// <param name="value">Valor a verificar</param>
    /// <typeparam name="T">Tipo</typeparam>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(T? value)
    {
        if (IsNull(value))
        {
            return true;

        }
        else if (value is string)
        {
            return string.IsNullOrEmpty(value as string);
        }
        return value!.Equals(default(T));
    }

    /// <summary>
    /// Verificar se o valor é nulo
    /// </summary>
    /// <param name="value">Valor a verificar</param>
    /// <typeparam name="T">Tipo</typeparam>
    public static bool IsNull<T>(T? value)
    {
        if (value is ValueType)
        {
            return false;
        }
        return value is null;
    }

    /// <summary>
    /// Obtém o valor de determinada propriedade do objeto recebido.
    /// </summary>
    /// <param name="entity">Entidade da qual será obtido o valor</param>
    /// <param name="propertyInfoObject">Campo da entidade</param>
    /// <returns>Valor de retorno</returns>
    public static object? GetPropertyValue(object entity,
                                          string propertyname)
    {
        PropertyInfo propertyinfo = entity.GetType().GetProperty(propertyname)
            ?? throw new ArgumentException($"Propriedade {propertyname} não existe em {entity.GetType().Name}.", nameof(propertyname));

        return propertyinfo.GetValue(entity);
    }

    /// <summary>
    /// Atribui o valor para uma determinada propriedade de um objeto
    /// </summary>
    /// <param name="source">Objecto a ser utilizado</param>
    /// <param name="propertyname">Nome da propriedade</param>
    /// <param name="value">Valor a ser atribuido</param>
    public static void SetPropertyValue(object source,
                                        string propertyname,
                                        object? value)
    {
        PropertyInfo propertyinfo = source.GetType().GetProperty(propertyname)
            ?? throw new ArgumentException($"Propriedade {propertyname} não existe em {source.GetType().Name}.", nameof(propertyname));

        propertyinfo.SetValue(source, value);
    }

    /// <summary>
    /// Obtém o valor de determinada propriedade do objeto recebido.
    /// </summary>
    /// <param name="entity">Entidade da qual será obtido o valor</param>
    /// <param name="propertyInfoObject">Campo da entidade</param>
    /// <returns>Valor de retorno</returns>
    public static object? GetPropertyValue(object entity,
                                          PropertyInfo propertyinfo)
    {
        return propertyinfo.GetValue(entity);
    }

    public static bool HasProperty(Type obj, string propertyName)
    {
        return obj.GetProperty(propertyName) != null;
    }

    public static bool HasProperty(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName) != null;
    }

}
