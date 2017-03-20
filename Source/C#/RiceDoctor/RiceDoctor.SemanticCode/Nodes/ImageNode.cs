using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class ImageNode : TextContainerNode
    {
        public ImageNode([NotNull] string url) : base("img")
        {
            Check.NotEmpty(url, nameof(url));

            AddAttribute("src", url);
            AddAttribute("alt", GetInnerText());
        }

        [NotNull]
        public string Src => Attributes["src"];

        [NotNull]
        public string Alt => Attributes["alt"];

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var attribute in Attributes)
                builder.Append($" {attribute.Key}=\"{attribute.Value}\"");

            return $"<img {builder} />";
        }
    }
}