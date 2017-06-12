using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryManager
{
    public enum QueryType
    {
        Class,
        Individual
    }

    public class Query
    {
        public Query(
            int weight,
            [NotNull] QueryContainerNode node,
            [NotNull] IReadOnlyDictionary<int, IReadOnlyCollection<QueryType>> results)
        {
            Check.NotNull(node, nameof(node));
            Check.NotEmpty(results, nameof(results));

            Weight = weight;
            Node = node;
            Results = results;
        }

        public int Weight { get; }

        [NotNull]
        public QueryContainerNode Node { get; }

        [NotNull]
        public IReadOnlyDictionary<int, IReadOnlyCollection<QueryType>> Results { get; }
    }
}