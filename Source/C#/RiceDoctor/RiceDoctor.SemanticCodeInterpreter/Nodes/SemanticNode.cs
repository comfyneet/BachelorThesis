using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCodeInterpreter
{
    public abstract class SemanticNode : Node<SemanticNode>
    {
        [NotNull]
        public abstract override string ToString();
    }
}