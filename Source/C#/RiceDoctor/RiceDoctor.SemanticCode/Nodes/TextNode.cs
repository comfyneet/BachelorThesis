using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class TextNode : SemanticNode
    {
        public TextNode([NotNull] string text)
        {
            Check.NotNull(text, nameof(text));

            Text = text;
        }

        [NotNull]
        public string Text { get; }

        public override string ToString()
        {
            var text = Text.Trim();

            if (!string.IsNullOrWhiteSpace(text))
            {
                if (Text.First() == ' ') text = ' ' + text;
                if (Text.Last() == ' ') text = text + ' ';
            }

            return text;
        }
    }
}