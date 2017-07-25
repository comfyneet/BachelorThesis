using System.Collections.Generic;
using RiceDoctor.RuleManager;

namespace RiceDoctor.InferenceEngine
{
    public class Path
    {
        public Path(List<Fact> known, List<LogicRule> chains)
        {
            Known = known;
            Chains = chains;
        }

        public List<Fact> Known { get; }

        public List<LogicRule> Chains { get; }
    }
}