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
                if (child is TextNode)
                {
                    if (child.PrevNode != null && !(child.PrevNode is TextNode ||
                                                    child.PrevNode is OptionNode ||
                                                    child.PrevNode is AlternativeNode ||
                                                    child.PrevNode is DiscardNode))
                        builder.Append(" +");

                    builder.Append(child);

                    if (child.NextNode != null && !(child.NextNode is TextNode ||
                                                    child.NextNode is OptionNode ||
                                                    child.NextNode is AlternativeNode ||
                                                    child.NextNode is DiscardNode))
                        builder.Append(" +");
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