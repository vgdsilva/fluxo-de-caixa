using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using WebAPI.Data.Types;

namespace WebAPI.Core.Utils.Classes;

public class EntityUtils
{
    /// <summary>
    /// Busca as entidades do sistema
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Type> GetEntities(string? provider = null)
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name!;

        string nameSpaceName = typeof(Entity).Namespace!;

        Func<Type, bool> filter = type => typeof(Entity).IsAssignableFrom(type) && !type.IsAbstract;

        IEnumerable<Type> types = ReflectionUtils.FindTypeInAssemblyNamespace(assemblyName, nameSpaceName, filter);

        return types;
    }

    /// <summary>
    /// Verifica se o tipo é uma entidade de banco de dados
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsEntity(Type type)
    {
        return typeof(Entity).IsAssignableFrom(type);
    }

    /// <summary>
    /// Verifica se o objeto informado possui tipo de entidade
    /// </summary>
    /// <param name="obj">Instancia de objeto</param>
    /// <returns></returns>
    public static bool IsEntity(object? obj)
    {
        return obj != null && IsEntity(obj.GetType());
    }

    /// <summary>
    /// Busca o valor da chave primaria para o objeto informado
    /// </summary>
    /// <param name="entity">instancia do objeto a ser verificado</param>
    /// <returns></returns>
    public static long GetPrimaryKeyValue(object entity)
    {
        PropertyInfo propertyInfo = ReflectionUtils.GetFirstPropertyWithAnnotation(entity.GetType(), typeof(KeyAttribute));
        return Convert.ToInt64(propertyInfo?.GetValue(entity));
    }

    /// <summary>
    /// Pega o nome da foreign key na anotação ForeignKey
    /// </summary>
    /// <param name="property">Propriedade verificada</param>
    /// <returns>Nome do atributo de foreign key</returns>
    public static string? GetForeignKeyIdFieldName(PropertyInfo property)
    {
        if (ReflectionUtils.HasAnnotation<ForeignKeyAttribute>(property))
        {
            ForeignKeyAttribute foreignKeyAttribute = ReflectionUtils.GetAttribute<ForeignKeyAttribute>(property);
            return foreignKeyAttribute.Name;
        }
        return null;
    }
}
