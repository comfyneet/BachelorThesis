using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryAnalysis
{
    public abstract class QueryNode : Node<QueryNode>
    {
        [NotNull]
        public abstract override string ToString();
    }
}