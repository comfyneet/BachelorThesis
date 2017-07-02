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
        IReadOnlyCollection<LogicRule> InferredLogicRules { get; }

        [NotNull]
        IReadOnlyCollection<KeyValuePair<IndividualFact, RelationRule>> InferredRelationRules { get; }

        int AddFactsToKnown([NotNull] params Fact[] facts);

        void HandleGuessableFacts([NotNull] IReadOnlyCollection<Tuple<Fact, bool?>> guessableFacts);

        [NotNull]
        Response Infer();

        [NotNull]
        IReadOnlyCollection<Tuple<double, LogicRule, IReadOnlyList<Fact>>> GetIncompleteRules();
    }
}