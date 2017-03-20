using System.Text;
using JetBrains.Annotations;

namespace RiceDoctor.SemanticCode
{
    public class TextContainerNode : SemanticContainerNode
    {
        public TextContainerNode([NotNull] string tag) : base(tag)
        {
        }

        [NotNull]
        protected string GetInnerText()
        {
            var builder = new StringBuilder();

            foreach (var node in ChildNodes)
                if (node is TextContainerNode textContainer)
                    builder.Append(textContainer.GetInnerText());
                else builder.Append(node);

            return builder.ToString();
        }
    }
}