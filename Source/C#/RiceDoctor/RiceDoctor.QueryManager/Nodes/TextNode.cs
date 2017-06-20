using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryManager
{
    public class TextNode : QueryNode
    {
        [NotNull] private readonly string _words;

        public TextNode([NotNull] string words)
        {
            Check.NotNull(words, nameof(words));

            _words = words;
        }

        public override string ToString()
        {
            var words = _words.Split(' ');

            return string.Join(" +", words);
        }
    }
}