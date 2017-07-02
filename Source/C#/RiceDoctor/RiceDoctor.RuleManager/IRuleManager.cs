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
        IReadOnlyCollection<Rule> Rules { get; }

        bool CanClassCaptureFact([NotNull] Class type, [NotNull] Fact fact);
    }
}