using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;

namespace RiceDoctor.InferenceEngine
{
    public interface IInferenceEngine
    {
        bool AddFactToKnown([NotNull] Fact fact);

        int AddFactsToKnown([NotNull] IReadOnlyCollection<Fact> facts);

        bool Infer([NotNull] string className);
    }
}