namespace RiceDoctor.ConversationalAgent
{
    public class OptionWildcardNode : ChatNode
    {
        public override string ToString()
        {
            return "(.*)";
        }
    }
}