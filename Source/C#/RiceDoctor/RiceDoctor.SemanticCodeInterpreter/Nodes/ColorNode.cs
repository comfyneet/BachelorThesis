using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCodeInterpreter
{
    public class ColorNode : TextContainerNode
    {
        public ColorNode([NotNull] string color) : base("font")
        {
            Check.NotEmpty(color, nameof(color));

            AddAttribute("color", color);
        }

        [NotNull]
        public string Color => Attributes["color"];
    }
}