using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;

namespace RiceDoctor.RuleManager
{
    public interface IRuleManager
    {
        [NotNull]
        IReadOnlyList<Problem> Problems { get; }

        [NotNull]
        IReadOnlyCollection<LogicRule> LogicRules { get; }

        [NotNull]
        IReadOnlyCollection<string> RelationRules { get; }

        bool CanFactCaptureClass([NotNull] Fact fact, [NotNull] Class type);
    }
}