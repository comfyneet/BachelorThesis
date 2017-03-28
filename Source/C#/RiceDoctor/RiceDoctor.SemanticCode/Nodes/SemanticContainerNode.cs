using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public abstract class SemanticContainerNode : ContainerNode<SemanticNode>
    {
        [NotNull] private readonly Dictionary<string, string> _attributes;

        protected SemanticContainerNode([NotNull] string tag)
        {
            Check.NotEmpty(tag, nameof(tag));

            Tag = tag;
            _attributes = new Dictionary<string, string>();
        }

        [NotNull]
        public string Tag { get; }

        [NotNull]
        public IReadOnlyDictionary<string, string> Attributes => _attributes;

        public void AddAttribute([NotNull] string key, [NotNull] string value)
        {
            Check.NotEmpty(key, nameof(key));
            Check.NotEmpty(value, nameof(value));

            _attributes.Add(key, value);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"<{Tag}");

            foreach (var attribute in Attributes)
                builder.Append($" {attribute.Key}=\"{attribute.Value}\"");

            builder.Append(">");

            if (Tag == "code" && ChildNodes.Count > 0) builder.Append("\r\n");

            foreach (var child in ChildNodes)
                builder.Append(child);

            builder.Append($"</{Tag}>");

            return builder.ToString();
        }
    }
}