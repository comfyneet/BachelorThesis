using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.InferenceEngine
{
    public class Engine : IInferenceEngine
    {
        [NotNull] private readonly IList<IndividualFact> _inferredIndividualFacts;
        [NotNull] private readonly IList<LogicRule> _inferredLogicRules;
        [NotNull] private readonly IList<Fact> _knownFacts;
        [NotNull] private readonly IOntologyManager _ontologyManager;
        [NotNull] private readonly IRuleManager _ruleManager;

        public Engine([NotNull] IRuleManager ruleManager, [NotNull] IOntologyManager ontologyManager)
        {
            Check.NotNull(ruleManager, nameof(ruleManager));
            Check.NotNull(ontologyManager, nameof(ontologyManager));

            _ruleManager = ruleManager;
            _ontologyManager = ontologyManager;
            _inferredLogicRules = new List<LogicRule>();
            _inferredIndividualFacts = new List<IndividualFact>();
            _knownFacts = new List<Fact>();
        }

        public IReadOnlyCollection<Fact> Infer(Request request)
        {
            Check.NotNull(request, nameof(request));

            if (request.KnownFacts != null)
                AddFactsToKnown(request.KnownFacts);

            _inferredLogicRules.Clear();

            while (true)
            {
                var resultFacts = GetRequestFactsInKnown(request);
                if (resultFacts != null) return resultFacts;

                var hasNewFacts = false;

                foreach (var rule in _ruleManager.LogicRules)
                {
                    hasNewFacts = InferLogicRule(rule);
                    if (hasNewFacts) break;
                }

                if (!hasNewFacts)
                    foreach (var fact in _knownFacts)
                    {
                        if (!(fact is IndividualFact individualFact)) continue;
                        hasNewFacts = InferIndividualFact(individualFact);
                        if (hasNewFacts) break;
                    }

                if (hasNewFacts) continue;

                break;
            }

            return null;
        }

        public IReadOnlyCollection<Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
            GetIncompleteFacts(Request request)
        {
            Check.NotNull(request, nameof(request));

            if (request.KnownFacts != null)
                AddFactsToKnown(request.KnownFacts);

            var factName = request.RequestType == RequestType.IndividualFact
                ? request.FactName
                : '"' + request.FactName + '"';

            var incompleteFacts = new List<Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>();
            foreach (var rule in _ruleManager.LogicRules)
            {
                var resultFacts = rule.Conclusions
                    .Where(f => f.Name == factName)
                    .ToList();

                if (resultFacts.Count <= 0) continue;

                var neededComplementFacts = new List<Fact>(rule.Hypotheses);
                foreach (var hypothesis in rule.Hypotheses)
                    if (_knownFacts.Contains(hypothesis)) neededComplementFacts.Remove(hypothesis);

                var priority = rule.CertaintyFactor * (1 - (double) neededComplementFacts.Count / rule.Hypotheses.Count);

                if (priority > 0)
                    incompleteFacts.Add(
                        new Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>(priority,
                            neededComplementFacts, resultFacts));
            }

            if (incompleteFacts.Count == 0) return null;

            return incompleteFacts
                .OrderByDescending(r => r.Item1)
                .ToList();
        }

        public bool AddFactToKnown(Fact fact)
        {
            Check.NotNull(fact, nameof(fact));

            if (_knownFacts.Any(f => f.Equals(fact)))
                return false;

            _knownFacts.Add(fact);
            return true;
        }

        public int AddFactsToKnown(IReadOnlyCollection<Fact> facts)
        {
            Check.NotEmpty(facts, nameof(facts));

            var count = 0;
            foreach (var fact in facts)
            {
                var added = AddFactToKnown(fact);
                if (added) count++;
            }

            return count;
        }

        [CanBeNull]
        private IReadOnlyCollection<Fact> GetRequestFactsInKnown([NotNull] Request request)
        {
            Check.NotNull(request, nameof(request));

            var factName = request.RequestType == RequestType.IndividualFact
                ? request.FactName
                : '"' + request.FactName + '"';

            var requestFacts = _knownFacts
                .Where(f => f.Name == factName)
                .ToList();

            return requestFacts.Count == 0 ? null : requestFacts;
        }

        private bool IsFactInKnown([NotNull] Fact fact)
        {
            Check.NotNull(fact, nameof(fact));

            return _knownFacts.Any(f => f.Equals(fact));
        }

        private bool AreFactsInKnown([NotNull] IReadOnlyCollection<Fact> facts)
        {
            Check.NotEmpty(facts, nameof(facts));

            return facts.All(IsFactInKnown);
        }

        private bool InferLogicRule([NotNull] LogicRule rule)
        {
            Check.NotNull(rule, nameof(rule));

            if (_inferredLogicRules.Any(rule.Equals)) return false;

            if (!AreFactsInKnown(rule.Hypotheses)) return false;

            _inferredLogicRules.Add(rule);

            return AddFactsToKnown(rule.Conclusions) > 0;
        }

        private bool InferIndividualFact([NotNull] IndividualFact individualFact)
        {
            Check.NotNull(individualFact, nameof(individualFact));

            if (_inferredIndividualFacts.Any(individualFact.Equals)) return false;

            _inferredIndividualFacts.Add(individualFact);

            var hasNewFacts = false;
            var relationValues = _ontologyManager.GetRelationValues(individualFact.Individual);
            if (relationValues != null)
                foreach (var relationValue in relationValues)
                foreach (var individual in relationValue.Value)
                {
                    var directClass = individual.DirectClass;
                    if (directClass != null)
                    {
                        var newIndividualFact = new IndividualFact(directClass.Id, individual.Id);

                        if (AddFactToKnown(newIndividualFact))
                            hasNewFacts = true;
                    }
                }

            return hasNewFacts;
        }
    }
}