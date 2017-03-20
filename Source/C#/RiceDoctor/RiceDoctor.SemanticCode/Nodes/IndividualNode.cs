using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class IndividualNode : TextContainerNode
    {
        public IndividualNode([NotNull] string className, [NotNull] string individualName) : base("a")
        {
            Check.NotEmpty(className, nameof(className));
            Check.NotEmpty(individualName, nameof(individualName));

            Class = className;
            Individual = individualName;
            AddAttribute("href", $"{SemanticParser.OntologyLink}{Class}/{Individual}");
        }

        [NotNull]
        public string Class { get; }

        [NotNull]
        public string Individual { get; }

        [NotNull]
        public string Href => Attributes["href"];
    }
}