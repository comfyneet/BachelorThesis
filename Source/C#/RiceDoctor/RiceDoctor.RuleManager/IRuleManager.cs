using System.Collections.Generic;

namespace RiceDoctor.RuleManager
{
    public interface IRuleManager
    {
        IReadOnlyCollection<LogicRule> LogicRules { get; }
    }
}