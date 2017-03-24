using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;

namespace RiceDoctor.InferenceEngine
{
    public interface IInferenceEngine
    {
        bool AddFactToKnown([NotNull] Fact fact);

        int AddFactsToKnown([NotNull] IReadOnlyCollection<Fact> facts);

        [CanBeNull]
        IReadOnlyCollection<Fact> Infer([NotNull] Request request);

        [CanBeNull]
        IReadOnlyCollection<Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>> GetIncompleteFacts(
            [NotNull] Request request);
    }
}