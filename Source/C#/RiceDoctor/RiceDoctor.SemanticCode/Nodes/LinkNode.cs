using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class LinkNode : TextContainerNode
    {
        public LinkNode([NotNull] string url) : base("a")
        {
            Check.NotEmpty(url, nameof(url));

            AddAttribute("href", url);
        }

        [NotNull]
        public string Href => Attributes["href"];
    }
}