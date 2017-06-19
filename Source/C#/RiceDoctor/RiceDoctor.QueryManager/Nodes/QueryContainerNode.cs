using System.Text;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryManager
{
    public class QueryContainerNode : ContainerNode<QueryNode>
    {
        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var child in ChildNodes)
                if (child is TextNode textChild)
                {
                    if (textChild.ToString() == ",")
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        if (child.PrevNode != null &&
                            !(child.PrevNode is TextNode prevChild && prevChild.ToString() == ","))
                            builder.Append(' ');
                        builder.Append(child);
                        if (child.NextNode != null) builder.Append(' ');
                    }
                }
                else if (child is AlternativeNode)
                {
                    if (child.PrevNode != null && !(child.PrevNode is AlternativeNode || child.PrevNode is TextNode))
                        builder.Append(' ');
                    builder.Append(child);
                    if (child.NextNode != null && !(child.NextNode is AlternativeNode || child.NextNode is TextNode))
                        builder.Append(' ');
                }
                else
                {
                    builder.Append(child);
                }

            builder.Append('$');

            return builder.ToString();
        }
    }
}