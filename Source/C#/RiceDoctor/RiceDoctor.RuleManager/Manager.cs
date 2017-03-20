using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class Manager : IRuleManager
    {
        public Manager([NotNull] string logicRuleData)
        {
            Check.NotEmpty(logicRuleData, nameof(logicRuleData));

            var logicLexer = new LogicLexer(logicRuleData);
            var logicParser = new LogicParser(logicLexer);
            LogicRules = logicParser.Parse();
        }

        [NotNull]
        public IReadOnlyCollection<LogicRule> LogicRules { get; }
    }
}