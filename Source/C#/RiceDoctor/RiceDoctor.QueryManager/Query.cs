using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryManager
{
    public class Query
    {
        [NotNull] private readonly Regex _regex;

        public Query(
            int weight,
            [NotNull] QueryContainerNode node,
            [NotNull] IReadOnlyDictionary<int, IReadOnlyCollection<QueryType>> resultTypes)
        {
            Check.NotNull(node, nameof(node));
            Check.NotEmpty(resultTypes, nameof(resultTypes));

            Weight = weight;
            Node = node;
            ResultTypes = resultTypes;
            _regex = new Regex(Node.ToString());
        }

        public int Weight { get; }

        [NotNull]
        public QueryContainerNode Node { get; }

        [NotNull]
        public IReadOnlyDictionary<int, IReadOnlyCollection<QueryType>> ResultTypes { get; }

        [CanBeNull]
        public IReadOnlyList<string> Match(string input)
        {
            var unaccentKeywords = input.ToLower().RemoveAccents();

            var match = _regex.Match(unaccentKeywords);
            if (!match.Success || match.Groups.Count < 2) return null;

            var results = new List<string>();
            for (var i = 1; i < match.Groups.Count; ++i)
            {
                var value = match.Groups[i].Value.Trim();
                var result = Regex.Replace(value, @"\s+", " ");

                results.Add(result);
            }

            return results;
        }
    }
}