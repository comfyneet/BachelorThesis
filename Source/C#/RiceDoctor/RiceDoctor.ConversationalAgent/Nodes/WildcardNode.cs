namespace RiceDoctor.ConversationalAgent
{
    public class WildcardNode : ChatNode
    {
        public override string ToString()
        {
            return "(.+)";
        }
    }
}