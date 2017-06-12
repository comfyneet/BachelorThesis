namespace RiceDoctor.QueryManager
{
    public class OptionWildcardNode : QueryNode
    {
        public override string ToString()
        {
            return "(.*)";
        }
    }
}