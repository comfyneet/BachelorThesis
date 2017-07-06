using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryAnalysis
{
    public class Query
    {
        [NotNull] private readonly Regex _regex;

        public Query(int weight, [NotNull] QueryContainerNode node)
        {
            Check.NotNull(node, nameof(node));

            Weight = weight;
            Node = node;
            _regex = new Regex(Node.ToString());
        }

        public int Weight { get; }

        [NotNull]
        public QueryContainerNode Node { get; }

        [CanBeNull]
        public IReadOnlyList<string> Match(string input)
        {
            var match = _regex.Match(input.ToLower().RemoveDuplicateSpaces());
            if (!match.Success || match.Groups.Count < 2) return null;

            var terms = new List<string>();
            for (var i = 1; i < match.Groups.Count; ++i)
            {
                var value = match.Groups[i].Value;
                if (string.IsNullOrWhiteSpace(value)) continue;

                var term = value.Trim().RemoveDuplicateSpaces();
                terms.Add(term);
            }

            return terms.Count == 0 ? null : terms;
        }
    }
}