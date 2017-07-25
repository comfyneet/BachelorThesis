using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;

namespace RiceDoctor.InferenceEngine
{
    public interface IInferenceEngine
    {
        [NotNull]
        IReadOnlyCollection<Rule> Rules { get; }

        [NotNull]
        IReadOnlyCollection<LogicRule> InferredRules { get; }

        [NotNull]
        IReadOnlyCollection<KeyValuePair<IndividualFact, RelationRule>> InferredRelations { get; }

        int AddFactsToKnown(bool reliable, [NotNull] params Fact[] facts);

        void HandleGuessableFacts([NotNull] IReadOnlyCollection<Tuple<Fact, bool?>> guessableFacts);

        [NotNull]
        Response Infer();
    }
}