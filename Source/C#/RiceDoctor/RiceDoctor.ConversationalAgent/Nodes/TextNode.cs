using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.ConversationalAgent
{
    public class TextNode : ChatNode
    {
        [NotNull] private readonly string _words;

        public TextNode([NotNull] string words)
        {
            Check.NotNull(words, nameof(words));

            _words = words;
        }

        public override string ToString()
        {
            return _words;
        }
    }
}