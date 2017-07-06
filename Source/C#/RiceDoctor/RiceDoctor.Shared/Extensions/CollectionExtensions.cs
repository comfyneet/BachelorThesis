using System;
using System.Collections.Generic;
using System.Linq;

namespace RiceDoctor.Shared
{
    public static class CollectionExtensions
    {
        private static readonly Random _random = new Random();

        public static bool ScrambledEqual<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (ReferenceEquals(first, second)) return true;
            if (ReferenceEquals(null, second)) return false;

            var elementCount = new Dictionary<T, int>();
            var nullCount = 0;

            foreach (var s in first)
                if (s == null) nullCount++;
                else if (elementCount.ContainsKey(s))
                    elementCount[s]++;
                else elementCount.Add(s, 1);

            foreach (var s in second)
                if (s == null) nullCount--;
                else if (elementCount.ContainsKey(s)) elementCount[s]--;
                else return false;

            return elementCount.Values.All(c => c == 0) && nullCount == 0;
        }

        public static int GetOrderIndependentHashCode<TSource>(this IEnumerable<TSource> source)
        {
            var hashCode = source
                .Select(element => EqualityComparer<TSource>.Default.GetHashCode(element))
                .Where(hash => hash != 0)
                .Aggregate(17, (current, hash) => unchecked(current * hash));

            return hashCode;
        }

        public static T RandomElement<T>(this IList<T> source)
        {
            if (source.Count == 0) throw new IndexOutOfRangeException(nameof(source));
            return source[_random.Next(source.Count)];
        }

        public static IReadOnlyCollection<IReadOnlyCollection<T>> GetSubsets<T>(this IList<T> set)
        {
            var subsets = new List<IReadOnlyCollection<T>>();

            for (var i = 1; i < 1 << set.Count; i++)
            {
                var subset = new List<T>();
                for (var j = 0; j < set.Count; j++)
                    if ((i & (1 << j)) > 0)
                        subset.Add(set[j]);

                subsets.Add(subset);
            }

            return subsets;
        }
    }
}