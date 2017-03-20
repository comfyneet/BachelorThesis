using System.Collections.Generic;
using System.Linq;

namespace RiceDoctor.Shared
{
    public static class EnumerableExtensions
    {
        public static bool ScrambledEquals<T>(this IEnumerable<T> list, IEnumerable<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(list, other)) return true;

            var count = new Dictionary<T, int>();

            foreach (var s in list)
                if (count.ContainsKey(s))
                    count[s]++;
                else count.Add(s, 1);

            foreach (var s in other)
                if (count.ContainsKey(s)) count[s]--;
                else return false;

            return count.Values.All(c => c == 0);
        }
    }
}