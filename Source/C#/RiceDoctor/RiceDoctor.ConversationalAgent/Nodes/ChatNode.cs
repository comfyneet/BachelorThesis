using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.ConversationalAgent
{
    public abstract class ChatNode : Node<ChatNode>
    {
        [NotNull]
        public abstract override string ToString();
    }
}