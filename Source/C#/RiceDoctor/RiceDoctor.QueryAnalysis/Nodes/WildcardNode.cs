namespace RiceDoctor.QueryAnalysis
{
    public class WildcardNode : QueryNode
    {
        public override string ToString()
        {
            return "(.+)";
        }
    }
}