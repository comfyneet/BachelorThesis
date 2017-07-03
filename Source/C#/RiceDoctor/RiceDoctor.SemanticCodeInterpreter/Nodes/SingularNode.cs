using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCodeInterpreter
{
    public class SingularNode : SemanticNode
    {
        public SingularNode([NotNull] string tag)
        {
            Check.NotEmpty(tag, nameof(tag));

            Tag = tag;
        }

        public string Tag { get; }

        public override string ToString()
        {
            return $"<{Tag}/>";
        }
    }
}