using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public enum ImageType
    {
        Online,
        Static
    }

    public class ImageNode : TextContainerNode
    {
        public ImageNode([NotNull] string url, ImageType type) : base("img")
        {
            Check.NotEmpty(url, nameof(url));

            Type = type;
            AddAttribute("src", url);
            AddAttribute("alt", GetInnerText());
        }

        public ImageType Type { get; }

        [NotNull]
        public string Src => Attributes["src"];

        [NotNull]
        public string Alt => Attributes["alt"];

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var attribute in Attributes)
                if (attribute.Key == "src" && Type == ImageType.Static)
                    builder.Append($" {attribute.Key}=\"{SemanticParser.StaticImageLink}{attribute.Value}\"");
                else builder.Append($" {attribute.Key}=\"{attribute.Value}\"");

            return $"<img {builder} />";
        }
    }
}