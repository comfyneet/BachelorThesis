using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCodeInterpreter
{
    public class ClassNode : TextContainerNode
    {
        public ClassNode([NotNull] string className) : base("a")
        {
            Check.NotEmpty(className, nameof(className));

            Class = className;
            AddAttribute("href", $"{SemanticParser.ClassLink}{Class}");

            var icon = new TextContainerNode("span");
            icon.AddAttribute("class", "glyphicon glyphicon-copyright-mark");
            Prepend(new TextNode("&nbsp;"));
            Prepend(icon);
        }

        [NotNull]
        public string Class { get; }

        [NotNull]
        public string Href => Attributes["href"];
    }
}