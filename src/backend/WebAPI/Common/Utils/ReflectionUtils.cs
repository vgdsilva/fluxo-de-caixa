using System.Collections.Concurrent;
using System.Reflection;

namespace WebAPI.Common.Utils
{
    public class ReflectionUtils
    {
        /// <summary>
        /// Verifica se a propriedade possui determinada annotação
        /// </summary>
        /// <param name="property">Propriedade a ser verificada</param>
        /// <param name="annotationType">Anotação a ser buscada</param>
        public static bool HasAnnotation(PropertyInfo property, Type annotationType)
        {
            return Attribute.IsDefined(property, annotationType, true);
        }

        /// <summary>
        /// Verifica se a propriedade possui determinada annotação
        /// </summary>
        /// <param name="property">Propriedade a ser verificada</param>
        /// <typeparam name="T">Anotação a ser buscada</typeparam>
        public static bool HasAnnotation<T>(PropertyInfo property) where T : Attribute
        {
            return HasAnnotation(property, typeof(T));
        }

        /// <summary>
        /// Verifica se o tipo informado possui anotação
        /// </summary>
        /// <param name="type">Tipo a ser verificado</param>
        /// <param name="annotationType">Tipo de anotação</param>
        /// <returns></returns>
        public static bool HasAnnotation(Type type, Type annotationType)
        {
            return Attribute.IsDefined(type, annotationType, true);
        }

        /// <summary>
        /// Verifica se o tipo informado possui a anotação
        /// </summary>
        /// <param name="type">Tipo a ser verificado</param>
        /// <typeparam name="T">Tipo da anotação</typeparam>
        /// <returns></returns>
        public static bool HasAnnotation<T>(Type type) where T : Attribute
        {
            return Attribute.IsDefined(type, typeof(T), true);
        }

        /// <summary>
        /// Obtém as entidades do Assembly com o NameSpace recebido como
        /// parâmetro.
        /// </summary>
        /// <param name="assemblyname">Nome do Assembly.</param>
        /// <param name="namespacename">Nome do NameSpace.</param>
        public static IEnumerable<Type> FindTypeInAssemblyNamespace(string assemblyname, string namespacename, Func<Type, bool>? filter = null)
        {
            Assembly entitiesassembly = LoadAssembly(assemblyname);
            return FindInAssemblyNamespace(entitiesassembly, namespacename, filter);
        }


        public static Assembly LoadAssembly(string assemblyname)
        {
            Assembly entitiesassembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault(a => a.GetName().Name == assemblyname) ?? Assembly.Load(new AssemblyName(assemblyname));
            return entitiesassembly;
        }

        /// <summary>
        /// Obtem os tipos declarados em determinado namespace.
        /// </summary>
        /// <param name="assembly">Assembly de onde serão especificados os
        /// tipos</param>
        /// <param name="nameSpace">Namespace a ser verificado.</param>
        /// <returns>List de tipos</returns>
        public static IEnumerable<Type> FindInAssemblyNamespace(Assembly assembly, string nameSpace, Func<Type, bool>? filter = null)
        {
            return
                assembly.GetTypes()
                .Where(t => string.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                .Where(filter ?? (_ => true))
                .ToList();
        }

        private static readonly ConcurrentDictionary<(Type, Type), List<PropertyInfo>> GetPropertyWithAnnotation_Cache = new();

        /// <summary>
        /// Busca propriedades com determinada anotação
        /// </summary>
        /// <param name="sourceType">Tipo do objeto a ser verificado</param>
        /// <param name="annotationType">Tipo da anotação a ser procurada</param>
        /// <returns>Lista de propriedades encontradas com o tipo informado</returns>
        public static List<PropertyInfo> GetPropertyWithAnnotation(Type sourceType, Type annotationType)
        {
            if (GetPropertyWithAnnotation_Cache.TryGetValue((sourceType, annotationType), out List<PropertyInfo>? properties))
                return properties;

            properties = [];

            foreach (PropertyInfo propertyInfo in TypeUtils.GetProperties(sourceType))
                if (Attribute.IsDefined(propertyInfo, annotationType, true))
                    properties.Add(propertyInfo);

            GetPropertyWithAnnotation_Cache[(sourceType, annotationType)] = properties;

            return properties;
        }

        public static List<PropertyInfo> GetPropertyWithAnnotation<T>(Type sourceType) => GetPropertyWithAnnotation(sourceType, typeof(T));

        /// <summary>
        /// Busca a primeira propriedade com a anotação informada
        /// </summary>
        /// <param name="sourceType">Tipo a ser verificado</param>
        /// <param name="annotationType">Anotação a ser procurada</param>
        /// <returns></returns>
        public static PropertyInfo GetFirstPropertyWithAnnotation(Type sourceType, Type annotationType) => GetPropertyWithAnnotation(sourceType, annotationType).First();


        /// <summary>
        /// Busca a primeira propriedade com a anotação informada
        /// </summary>
        /// <param name="type">Tipo a ser informado</param>
        /// <typeparam name="T">Tipo da anotação</typeparam>
        /// <returns></returns>
        public static T GetAttribute<T>(Type type) where T : Attribute
        {
            Type annotationType = typeof(T);

            T attribute = (T)type.GetCustomAttributes(false).First(a => a.GetType() == annotationType);
            return attribute;
        }

        /// <summary>
        /// Busca metadado de anotação
        /// </summary>
        /// <param name="property">Propriedade a ser conferida</param>
        /// <typeparam name="T">Tipo da anotação</typeparam>
        /// <returns></returns>
        public static T GetAttribute<T>(PropertyInfo property) where T : Attribute
        {
            Type annotationType = typeof(T);

            T attribute = (T)property.GetCustomAttributes(false).First(a => a.GetType() == annotationType);
            return attribute;
        }

        /// <summary>
        /// Verifica se o objeto possui uma propriedade com o nome especificado
        /// </summary>
        /// <param name="src">Objeto de referência</param>
        /// <param name="propName">Nome da propriedade</param>
        public static bool HasProperty(object src, string propName) => GetProperty(src, propName) != null;

        /// <summary>
        /// Busca uma propriedade em uma instancia de objeto
        /// </summary>
        /// <param name="src">Instância de objeto base</param>
        /// <param name="propName">Nome da propriedade</param>
        /// <returns>Dados da propriedade encontrada</returns>
        public static PropertyInfo? GetProperty(object src, string propName) => GetProperty(src.GetType(), propName);

        /// <summary>
        /// Verifica se o tipo possui uma propriedade com o nome informado
        /// </summary>
        /// <param name="srcType">Tipo de referência</param>
        /// <param name="propName">Nome da propriedade</param>
        public static bool HasProperty(Type srcType, string propName) => GetProperty(srcType, propName) != null;

        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> GetProperty_Cache = new();

        /// <summary>
        /// Busca uma propriedade em um tipo de objeto
        /// </summary>
        /// <param name="srcType">Tipo do objeto base</param>
        /// <param name="propName">Nome da propriedade</param>
        /// <returns>Dados da propriedade encontrada</returns>
        public static PropertyInfo? GetProperty(Type srcType, string propName)
        {
            if (GetProperty_Cache.TryGetValue((srcType, propName), out PropertyInfo? property))
                return property;

            property = srcType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            GetProperty_Cache[(srcType, propName)] = property;

            return property;
        }
    }
}
