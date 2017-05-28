using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;

namespace RiceDoctor.InferenceEngine
{
    public interface IInferenceEngine
    {
        [NotNull]
        IReadOnlyCollection<Relation> HighPriorityRelationRules { get; }

        [NotNull]
        IReadOnlyCollection<Relation> MidPriorityRelationRules { get; }

        [NotNull]
        IReadOnlyCollection<Relation> LowPriorityRelationRules { get; }

        [NotNull]
        IReadOnlyCollection<LogicRule> HighPriorityLogicRules { get; }

        [NotNull]
        IReadOnlyCollection<LogicRule> MidPriorityLogicRules { get; }

        [NotNull]
        IReadOnlyCollection<LogicRule> LowPriorityLogicRules { get; }

        [NotNull]
        IReadOnlyCollection<LogicRule> InferredLogicRules { get; }

        [NotNull]
        IReadOnlyCollection<KeyValuePair<IndividualFact, Relation>> InferredRelationRules { get; }

        int AddFactsToKnown([NotNull] params Fact[] facts);

        void HandleGuessableFact([NotNull] Tuple<Fact, bool?> guessableFact);

        [NotNull]
        Response Infer();

        [NotNull]
        IReadOnlyCollection<Tuple<double, LogicRule, IReadOnlyList<Fact>>> GetIncompleteRules();
    }
}