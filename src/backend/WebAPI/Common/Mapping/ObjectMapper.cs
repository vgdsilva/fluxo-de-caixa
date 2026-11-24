using System.Reflection;

namespace WebAPI.Common.Mapping
{
    public class ObjectMapper : IObjectMapper
    {

        public ObjectMapper()
        {
            
        }

        private static Func<object?, string, PropertyInfo, bool> GetCondition(Dictionary<string, List<string>>? dict)
        {
            return (sourceMember, keyEntityId, propInfo) =>
                                    (
                                        (dict == null && !ObjectUtils.IsNull(sourceMember))
                                        || (dict != null && dict.IsNullOrEmpty())
                                        || (dict != null && dict[keyEntityId].Contains(propInfo.Name.ToLower()))
                                    );
        }

        public TSource Map<TSource>(TSource source,
                                    TSource destination)
        {
            return source.CloneObject(destination, GetCondition(null));
        }

        public TSource Map<TSource>(TSource source,
                                    TSource destination,
                                    Dictionary<string, List<string>> dict)
        {
            return source.CloneObject(destination, GetCondition(dict));
        }

        public TSource Map<TSource>(TSource source)
        {
            return source.CloneObject(GetCondition(null));
        }
    }
}
