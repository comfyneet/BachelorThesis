using System.Text;

namespace RiceDoctor.ConversationalAgent
{
    public class MessageNode : ChatContainerNode
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