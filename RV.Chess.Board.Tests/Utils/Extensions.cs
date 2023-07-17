namespace RV.Chess.Board.Tests.Utils
{
    internal static class Extensions
    {
        internal static IEnumerable<TSource> Where<TSource>(this Span<TSource> source, Func<TSource, bool> predicate)
        {
            return source.ToArray().Where(predicate);
        }

        internal static TSource? Find<TSource>(this Span<TSource> source, Predicate<TSource> predicate)
        {
            return source.ToList().Find(predicate);
        }

        internal static IEnumerable<TResult> Select<TSource, TResult>(
            this Span<TSource> source, Func<TSource, TResult> selector)
        {
            return source.ToArray().Select(selector);
        }

        internal static TSource Single<TSource>(this Span<TSource> source, Func<TSource, bool> predicate)
        {
            return source.ToArray().Single(predicate);
        }

        internal static TSource? SingleOrDefault<TSource>(this Span<TSource> source, Func<TSource, bool> predicate)
        {
            return source.ToArray().SingleOrDefault(predicate);
        }

        internal static List<TSource> ToList<TSource>(this Span<TSource> source)
        {
            return Enumerable.ToList(source.ToArray());
        }

        internal static bool All<TSource>(this Span<TSource> source, Func<TSource, bool> predicate)
        {
            return source.ToArray().All(predicate);
        }
    }
}
