using System.Text;

namespace WebAPI.Core.Utils.Extensions;

public static class ListExtensions
{
    /// <summary>
    /// Verifica se a coleção está nulo ou vazia.
    /// </summary>
    /// <param name="source">Coleção a ser verificada</param>
    /// <typeparam name="T">Tipo da coleção</typeparam>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        if (source == null || !source.Any())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Formada a coleção para apresentação da forma [1,2,3].
    /// </summary>
    /// <param name="source">Coleção a ser formatada</param>
    /// <typeparam name="T">Tipo da coleção</typeparam>
    /// <returns></returns>
    public static string ToFormatedString<T>(this IEnumerable<T>? source)
    {
        if (source == null)
        {
            return "[]";
        }
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(string.Join(",", source));
        sb.Append("]");
        return sb.ToString();
    }

    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> items,
                                                           int partitionSize)
    {
        return items.Select((item, inx) => new { item, inx })
                    .GroupBy(x => x.inx / partitionSize)
                    .Select(g => g.Select(x => x.item));
    }

    public static IEnumerable<List<T>> SplitList<T>(this List<T> bigList, int nSize = 3)
    {
        for (int i = 0; i < bigList.Count; i += nSize)
        {
            yield return bigList.GetRange(i, Math.Min(nSize, bigList.Count - i));
        }
    }

}
