using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
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