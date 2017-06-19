using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryManager
{
    public class Query
    {
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
        }

        public int Weight { get; }

        [NotNull]
        public QueryContainerNode Node { get; }

        [NotNull]
        public IReadOnlyDictionary<int, IReadOnlyCollection<QueryType>> ResultTypes { get; }
    }
}