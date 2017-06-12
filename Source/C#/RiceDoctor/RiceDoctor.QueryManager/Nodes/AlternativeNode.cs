using System.Text;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryManager
{
    public class AlternativeNode : QueryContainerNode
    {
        public override Node<QueryNode> Append(Node<QueryNode> node)
        {
            var textNode = Check.IsType<TextNode>(node, nameof(node));

            return base.Append(textNode);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append("(?:");

            for (var i = 0; i < ChildNodes.Count; ++i)
            {
                if (i != 0) builder.Append('|');

                builder.Append((TextNode) ChildNodes[i]);
            }

            builder.Append(')');

            return builder.ToString();
        }
    }
}