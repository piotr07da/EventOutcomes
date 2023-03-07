namespace EventOutcomes;

internal static class EnumerableExtensions
{
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
    {
        var hs = new HashSet<T>();
        foreach (var e in enumerable)
        {
            hs.Add(e);
        }

        return hs;
    }

    public static T[] RangeFrom<T>(this T[] array, int startIndex)
    {
        return array.Range(startIndex, array.Length - 1);
    }

    public static T[] RangeTo<T>(this T[] array, int endIndex)
    {
        return array.Range(0, endIndex);
    }

    public static T[] Range<T>(this T[] array, int startIndex, int endIndex)
    {
        var result = new T[endIndex - startIndex + 1];
        Array.Copy(array, startIndex, result, 0, result.Length);
        return result;
    }
}
