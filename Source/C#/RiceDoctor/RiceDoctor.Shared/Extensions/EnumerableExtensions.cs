using System.Collections.Generic;
using System.Linq;

namespace RiceDoctor.Shared
{
    public static class EnumerableExtensions
    {
        public static bool ScrambledEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (ReferenceEquals(first, second)) return true;
            if (ReferenceEquals(null, second)) return false;

            var count = new Dictionary<TSource, int>();

            foreach (var s in first)
                if (count.ContainsKey(s))
                    count[s]++;
                else count.Add(s, 1);

            foreach (var s in second)
                if (count.ContainsKey(s)) count[s]--;
                else return false;

            return count.Values.All(c => c == 0);
        }

        public static int GetOrderIndependentHashCode<TSource>(this IEnumerable<TSource> source)
        {
            var hashCode = source
                .Select(element => EqualityComparer<TSource>.Default.GetHashCode(element))
                .Where(hash => hash != 0)
                .Aggregate(17, (current, hash) => unchecked(current * hash));

            return hashCode;
        }
    }
}