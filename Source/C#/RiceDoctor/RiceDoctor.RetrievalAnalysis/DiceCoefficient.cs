using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RetrievalAnalysis
{
    public static class DiceCoefficient
    {
        public static double Distance([NotNull] string s1, [NotNull] string s2)
        {
            Check.NotEmpty(s1, nameof(s1));
            Check.NotEmpty(s2, nameof(s2));

            var arr1 = Regex.Split(s1.Trim(), @"\s+");
            var arr2 = Regex.Split(s2.Trim(), @"\s+");

            var s1Tokens = arr1.Distinct().ToArray();
            var s1TermCount = s1Tokens.Length;

            var s2Tokens = arr2.Distinct().ToArray();
            var s2TermCount = s2Tokens.Length;

            var allTokens = s1Tokens.Concat(s2Tokens).Distinct().ToArray();
            var commonTerms = s1TermCount + s2TermCount - allTokens.Length;

            return 2.0 * commonTerms / (s1TermCount + s2TermCount);
        }
    }
}