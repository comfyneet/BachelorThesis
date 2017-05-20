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
        IReadOnlyCollection<Relation> RelationRules { get; }

        bool CanClassCaptureFact([NotNull] Class type, [NotNull] Fact fact);
    }
}