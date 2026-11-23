namespace WebAPI.Core.Utils.Interfaces;

public interface IObjectMapper
{
    TSource Map<TSource>(TSource source, TSource destination);

    TSource Map<TSource>(TSource source, TSource destination, Dictionary<string, List<string>> dict);

    TSource Map<TSource>(TSource source);
}
