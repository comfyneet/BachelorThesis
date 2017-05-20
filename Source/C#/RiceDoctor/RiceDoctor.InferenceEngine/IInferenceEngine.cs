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

        int AddFactsToKnown([NotNull] params Fact[] facts);

        void HandleGuessableFact([NotNull] Tuple<Fact, bool?> guessableFact);

        [NotNull]
        Response Infer();

        [CanBeNull]
        IReadOnlyCollection<ValueTuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
            GetIncompleteFacts();
    }
}