using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;

namespace RiceDoctor.InferenceEngine
{
    public interface IInferenceEngine
    {
        int AddFactsToKnown([NotNull] params Fact[] facts);

        [CanBeNull]
        IReadOnlyCollection<Fact> Infer([NotNull] Request request);

        [CanBeNull]
        IReadOnlyCollection<ValueTuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>> GetIncompleteFacts(
            [NotNull] Request request);
    }
}