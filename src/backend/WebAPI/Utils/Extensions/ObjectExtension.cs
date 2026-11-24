using System.Reflection;

namespace WebAPI.Utils.Extensions;

public static class ObjectExtension
{
    private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;

    public static T CloneObject<T>(this T objSource,
                                   Func<object?, string, PropertyInfo, bool>? filter = null)
    {
        T objTarget = (T)Activator.CreateInstance(typeof(T))!;
        return objSource.CloneObject(objTarget, filter);
    }

    public static T CloneObject<T>(this T objSource,
                                   T objTarget,
                                   Func<object?, string, PropertyInfo, bool>? filter = null)
    {
        return (T)InternalCloneObject(objSource, objTarget, new Dictionary<object, object?>(new ReferenceEqualityComparer()), filter)!;
    }

    private static object? InternalCloneObject(object? objSource,
                                              IDictionary<object, object?> visited,
                                              Func<object?, string, PropertyInfo, bool>? filter = null)
    {
        if (objSource == null)
        {
            return null;
        }

        object? objTarget;
        try
        {
            objTarget = Activator.CreateInstance(objSource.GetType());
        }
        catch (Exception e)
        {
            throw new Exception($"Erro ao instanciar classe {objSource.GetType().Name}", e);
        }
        return InternalCloneObject(objSource, objTarget, visited, filter);
    }

    private static object? InternalCloneObject(object? objSource,
                                              object? objTarget,
                                              IDictionary<object, object?> visited,
                                              Func<object?, string, PropertyInfo, bool>? filter = null)
    {

        if (objSource == null)
        {
            return null;
        }
        if (visited.TryGetValue(objSource, out object? alreadyCloned))
        {
            return alreadyCloned;
        }

        if (objTarget == null)
        {
            return null;
        }

        //Get the type of source object and create a new instance of that type
        Type typeSource = objSource.GetType();

        if (typeof(Delegate).IsAssignableFrom(typeSource))
        {
            return null;
        }

        if (TypeUtils.IsValueType(typeSource))
        {
            return objSource;
        }

        CopyFields(objSource, objTarget, typeSource, visited, filter);

        visited.Add(objSource, objTarget);
        return objTarget;
    }

    private static void CopyFields(object objSource,
                                   object objTarget,
                                   Type typeSource,
                                   IDictionary<object, object?> visited,
                                   Func<object?, string, PropertyInfo, bool>? filter = null)
    {
        //Get all the properties of source object type
        PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        try
        {
            string keyTableId = EntityUtils.IsEntity(objTarget)
                                ? $"{objTarget.GetType().Name}#{EntityUtils.GetPrimaryKeyValue(objTarget)}"
                                : string.Empty;

            //Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    object? objPropertyValue = property.GetValue(objSource, null);

                    if (filter != null && !filter.Invoke(objPropertyValue, keyTableId, property)) // TODO
                    {
                        continue;
                    }

                    //check whether property type is value type, enum or string type
                    if (!TypeUtils.IsValueType(property.PropertyType))
                    {
                        if (property.PropertyType.IsArray)
                        {
                            objPropertyValue = CloneArray(objPropertyValue, property.PropertyType, visited, filter);
                        }
                        //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                        else if (objPropertyValue != null)
                        {
                            objPropertyValue = InternalCloneObject(objPropertyValue, visited);
                        }

                    }
                    property.SetValue(objTarget, objPropertyValue, null);
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Erro ao copiar dados para entidade {typeSource.Name}", e);
        }
    }

    private static object? CloneArray(object? objPropertyValue,
                                     Type typeSource,
                                     IDictionary<object, object?> visited,
                                     Func<object?, string, PropertyInfo, bool>? filter = null)
    {
        if (objPropertyValue == null)
        {
            return null;
        }
        var cloneObject = CloneMethod.Invoke(objPropertyValue, null);
        var arrayType = typeSource.GetElementType();
        if (arrayType is not null && !TypeUtils.IsValueType(arrayType) && cloneObject is Array clonedArray)
        {
            for (int i = 0; i < clonedArray.Length; i++)
            {
                var clonedElement = InternalCloneObject(clonedArray.GetValue(i), visited, filter);
                clonedArray.SetValue(clonedElement, i);
            }
        }
        return cloneObject;
    }

}

public class ReferenceEqualityComparer : EqualityComparer<object>
{
    public override bool Equals(object? x, object? y)
    {
        return ReferenceEquals(x, y);
    }

    public override int GetHashCode(object obj)
    {
        return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
