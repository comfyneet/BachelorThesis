using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class ClassNode : TextContainerNode
    {
        public ClassNode([NotNull] string className) : base("a")
        {
            Check.NotEmpty(className, nameof(className));

            Class = className;
            AddAttribute("href", $"{SemanticParser.ClassLink}{Class}");
        }

        [NotNull]
        public string Class { get; }

        [NotNull]
        public string Href => Attributes["href"];
    }
}