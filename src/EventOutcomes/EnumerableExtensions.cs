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
}
