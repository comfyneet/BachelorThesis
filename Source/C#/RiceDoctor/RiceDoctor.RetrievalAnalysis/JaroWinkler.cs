using System;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RetrievalAnalysis
{
    // https://github.com/tdebatty/java-string-similarity#jaro-winkler
    public static class JaroWinkler
    {
        private const double Threshold = 0.7;
        private const int Three = 3;
        private const double JwCoef = 0.1;

        public static double Similarity([NotNull] string s1, [NotNull] string s2)
        {
            Check.NotNull(s1, nameof(s1));
            Check.NotNull(s2, nameof(s2));

            if (s1 == s2) return 1.0;

            var mtp = Matches(s1, s2);
            var m = mtp[0];
            if (m == 0) return 0f;

            var j = (double) (m / s1.Length + m / s2.Length + (m - mtp[1]) / m) / Three;
            var jw = j;

            if (j > Threshold) jw = j + Math.Min(JwCoef, 1.0 / mtp[Three]) * mtp[2] * (1 - j);
            return jw;
        }

        private static int[] Matches([NotNull] string s1, [NotNull] string s2)
        {
            Check.NotNull(s1, nameof(s1));
            Check.NotNull(s2, nameof(s2));

            string max, min;
            if (s1.Length > s2.Length)
            {
                max = s1;
                min = s2;
            }
            else
            {
                max = s2;
                min = s1;
            }
            var range = Math.Max(max.Length / 2 - 1, 0);

            var matchIndexes = Enumerable.Repeat(-1, min.Length).ToArray();

            var matchFlags = new bool[max.Length];
            var matches = 0;
            for (var mi = 0; mi < min.Length; mi++)
            {
                var c1 = min[mi];
                for (int xi = Math.Max(mi - range, 0),
                        xn = Math.Min(mi + range + 1, max.Length);
                    xi < xn;
                    xi++)
                    if (!matchFlags[xi] && c1 == max[xi])
                    {
                        matchIndexes[mi] = xi;
                        matchFlags[xi] = true;
                        matches++;
                        break;
                    }
            }

            var ms1 = new char[matches];
            var ms2 = new char[matches];
            for (int i = 0, si = 0; i < min.Length; i++)
                if (matchIndexes[i] != -1)
                {
                    ms1[si] = min[i];
                    si++;
                }

            for (int i = 0, si = 0; i < max.Length; i++)
                if (matchFlags[i])
                {
                    ms2[si] = max[i];
                    si++;
                }

            var transpositions = 0;
            for (var mi = 0; mi < ms1.Length; mi++)
                if (ms1[mi] != ms2[mi])
                    transpositions++;

            var prefix = 0;
            for (var mi = 0; mi < min.Length; mi++)
                if (s1[mi] == s2[mi])
                    prefix++;
                else
                    break;

            return new[] {matches, transpositions / 2, prefix, max.Length};
        }
    }
}