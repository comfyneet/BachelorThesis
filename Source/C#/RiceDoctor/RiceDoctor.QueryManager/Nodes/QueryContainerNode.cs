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
                builder.Append(child);

            builder.Append('$');

            return builder.ToString();
        }
    }
}