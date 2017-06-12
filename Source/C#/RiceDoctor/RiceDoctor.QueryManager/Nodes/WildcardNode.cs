namespace RiceDoctor.QueryManager
{
    public class WildcardNode : QueryNode
    {
        public override string ToString()
        {
            return "(.+)";
        }
    }
}