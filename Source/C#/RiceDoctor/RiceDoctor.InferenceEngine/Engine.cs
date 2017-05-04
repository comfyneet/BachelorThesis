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
        [NotNull] private readonly IDictionary<Fact, bool> _guessableFacts;
        [NotNull] private readonly IList<LogicRule> _inferableRules;
        [NotNull] private readonly IList<LogicRule> _inferredLogicRules;
        [NotNull] private readonly IList<IndividualFact> _inferredRelationalFacts;
        [NotNull] private readonly ISet<Fact> _knownFacts;
        [NotNull] private readonly IOntologyManager _ontologyManager;
        [NotNull] private readonly Request _request;
        [NotNull] private readonly IRuleManager _ruleManager;

        public Engine(
            [NotNull] IRuleManager ruleManager,
            [NotNull] IOntologyManager ontologyManager,
            [NotNull] Request request)
        {
            Check.NotNull(ruleManager, nameof(ruleManager));
            Check.NotNull(ontologyManager, nameof(ontologyManager));
            Check.NotNull(request, nameof(request));

            _ruleManager = ruleManager;
            _ontologyManager = ontologyManager;
            _request = request;

            _inferableRules = SortInferableRulesForRequest();
            _inferredLogicRules = new List<LogicRule>();
            _inferredRelationalFacts = new List<IndividualFact>();
            _knownFacts = new HashSet<Fact>();

            if (request.KnownFacts != null)
                AddFactsToKnown(request.KnownFacts.ToArray());
        }

        public IReadOnlyCollection<Fact> Infer()
        {
            var forwardChainingResults = InferForwardChaining();
            return forwardChainingResults ?? InferBackwardChaining();
        }

        public IReadOnlyCollection<ValueTuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
            GetIncompleteFacts()
        {
            var logicRules = SortInferableRulesForRequest();

            var incompleteFacts = new List<ValueTuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>();
            foreach (var rule in logicRules)
            {
                var resultFacts = new List<Fact>();
                foreach (var conclusion in rule.Conclusions)
                foreach (var goalType in _request.Problem.GoalTypes)
                    if (_ruleManager.CanFactCaptureClass(conclusion, goalType))
                    {
                        resultFacts.Add(conclusion);
                        break;
                    }

                //var resultFacts = rule.Conclusions
                //    .Where(f => request.Problem.DesireTypes.Contains(f.Name))
                //    .ToList();

                if (resultFacts.Count <= 0) continue;

                var missingFacts = new List<Fact>(rule.Hypotheses);
                foreach (var hypothesis in rule.Hypotheses)
                    if (_knownFacts.Contains(hypothesis)) missingFacts.Remove(hypothesis);

                var priority = rule.CertaintyFactor * (1 - (double) missingFacts.Count / rule.Hypotheses.Count);

                if (priority > 0)
                    incompleteFacts.Add((priority, missingFacts, resultFacts));
            }

            if (incompleteFacts.Count == 0) return null;

            return incompleteFacts
                .OrderByDescending(r => r.Item1)
                .ToList();
        }

        public int AddFactsToKnown(params Fact[] facts)
        {
            Check.NotEmpty(facts, nameof(facts));

            var count = 0;
            foreach (var fact in facts)
                if (!_knownFacts.Any(f => f.Equals(facts)))
                {
                    _knownFacts.Add(fact);
                    count++;
                }

            return count;
        }

        [CanBeNull]
        private IReadOnlyCollection<Fact> InferForwardChaining()
        {
            while (true)
            {
                var hasNewFacts = false;

                foreach (var rule in _inferableRules)
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

            var goalFacts = GetGoalFactsInKnown();
            return goalFacts;
        }

        [CanBeNull]
        private IReadOnlyCollection<Fact> InferBackwardChaining()
        {
            var logicRules = SortInferableRulesForRequest();

            foreach (var rule in logicRules)
            {
                var allHypothesesInKnown = true;
                foreach (var hypothesis in rule.Hypotheses)
                    if (!Backtrack(hypothesis, 0))
                    {
                        allHypothesesInKnown = false;
                        break;
                    }
                if (allHypothesesInKnown)
                    AddFactsToKnown(rule.Conclusions.ToArray());
            }

            var goalFacts = GetGoalFactsInKnown();
            return goalFacts;
        }

        private bool Backtrack([NotNull] Fact goal, int level)
        {
            if (IsFactInKnown(goal)) return true;

            var inferableRules = new List<LogicRule>();
            foreach (var rule in _ruleManager.LogicRules)
                if (rule.Conclusions.Any(c => c.Equals(goal)))
                    inferableRules.Add(rule);
            inferableRules = inferableRules.OrderBy(r => CountFactsInKnown(r.Hypotheses)).ToList();

            foreach (var rule in inferableRules)
            {
                var allHypothesesInKnown = true;
                foreach (var hypothesis in rule.Hypotheses)
                    if (!Backtrack(hypothesis, level + 1))
                    {
                        allHypothesesInKnown = false;
                        break;
                    }

                if (allHypothesesInKnown)
                {
                    AddFactsToKnown(goal);
                    return true;
                }
            }

            return false;
        }

        [NotNull]
        private IList<LogicRule> SortInferableRulesForRequest()
        {
            throw new NotImplementedException();

            var rules = new List<LogicRule>();

            foreach (var rule in _ruleManager.LogicRules)
                if (rule.Problems != null && rule.Problems.Contains(_request.Problem))
                    rules.Add(rule);

            return rules;
        }

        [CanBeNull]
        private IReadOnlyCollection<Fact> GetGoalFactsInKnown()
        {
            //var desireFacts = _knownFacts
            //    .Where(f => request.Problem.DesireTypes.Contains(f.Name))
            //    .ToList();

            var goalFacts = new List<Fact>();

            foreach (var fact in _knownFacts)
            foreach (var goalType in _request.Problem.GoalTypes)
                if (_ruleManager.CanFactCaptureClass(fact, goalType))
                {
                    goalFacts.Add(fact);
                    break;
                }

            return goalFacts.Count == 0 ? null : goalFacts;
        }

        private bool IsFactInKnown([NotNull] Fact fact)
        {
            Check.NotNull(fact, nameof(fact));

            return _knownFacts.Any(f => f.Equals(fact));
        }

        private int CountFactsInKnown([NotNull] IReadOnlyCollection<Fact> facts)
        {
            Check.NotEmpty(facts, nameof(facts));

            return facts.Count(IsFactInKnown);
        }

        private bool InferLogicRule([NotNull] LogicRule rule)
        {
            Check.NotNull(rule, nameof(rule));

            if (_inferredLogicRules.Any(rule.Equals)) return false;

            if (CountFactsInKnown(rule.Hypotheses) != rule.Hypotheses.Count) return false;

            _inferredLogicRules.Add(rule);

            return AddFactsToKnown(rule.Conclusions.ToArray()) > 0;
        }

        private bool InferIndividualFact([NotNull] IndividualFact individualFact)
        {
            Check.NotNull(individualFact, nameof(individualFact));

            if (_inferredRelationalFacts.Any(individualFact.Equals)) return false;

            _inferredRelationalFacts.Add(individualFact);

            var hasNewFacts = false;
            var relationValues = _ontologyManager.GetRelationValues(individualFact.Individual);
            if (relationValues != null)
                foreach (var relationValue in relationValues)
                {
                    if (!_ruleManager.RelationRules.Contains(relationValue.Key.Id))
                        continue;

                    foreach (var individual in relationValue.Value)
                    {
                        var directClass = individual.DirectClass;
                        if (directClass != null)
                        {
                            var newIndividualFact = new IndividualFact(directClass.Id, individual.Id);

                            if (AddFactsToKnown(newIndividualFact) > 0)
                                hasNewFacts = true;
                        }
                    }
                }

            return hasNewFacts;
        }
    }
}