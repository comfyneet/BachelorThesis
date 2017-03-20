using System;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class ListNode : SemanticContainerNode
    {
        public ListNode([NotNull] string tag) : base(tag)
        {
        }

        public ListNode([NotNull] string tag, [NotNull] string type) : base(tag)
        {
            Check.NotEmpty(type, nameof(type));

            switch (tag)
            {
                case "ol":
                    AddAttribute("type", type);
                    break;
                case "ul":
                    AddAttribute("style", $"list-style-type:{type}");
                    break;
                default:
                    throw new ArgumentException("Invalid list type.");
            }
        }

        public override Node<SemanticNode> Append(Node<SemanticNode> node)
        {
            var listItem = Check.IsType<ListItemNode>(node, nameof(node));

            return base.Append(listItem);
        }
    }
}