using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        [NotNull] private readonly IList<LogicRule> _highPriorityLogicRules;
        [NotNull] private readonly IList<LogicRule> _inferredLogicRules;
        [NotNull] private readonly IList<KeyValuePair<IndividualFact, Relation>> _inferredRelationalRules;
        [NotNull] private readonly IList<Fact> _knownFacts;
        [NotNull] private readonly IList<LogicRule> _lowPriorityLogicRules;
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

            (HighPriorityRelationRules, LowPriorityRelationRules, _highPriorityLogicRules, _lowPriorityLogicRules) =
                SortRulesByRequest();
            _inferredLogicRules = new List<LogicRule>();
            _inferredRelationalRules = new List<KeyValuePair<IndividualFact, Relation>>();
            _knownFacts = new List<Fact>();

            if (request.KnownFacts != null)
                AddFactsToKnown(request.KnownFacts.ToArray());
        }

        public IReadOnlyCollection<Fact> Infer()
        {
            var forwardChainingResults = InferForwardChaining();
            return forwardChainingResults /*?? InferBackwardChaining()*/;
        }

        public IReadOnlyCollection<ValueTuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
            GetIncompleteFacts()
        {
            throw new NotImplementedException();

            //var logicRules = SortRulesByRequest();

            //var incompleteFacts = new List<ValueTuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>();
            //foreach (var rule in logicRules)
            //{
            //    var resultFacts = new List<Fact>();
            //    foreach (var conclusion in rule.Conclusions)
            //    foreach (var goalType in _request.Problem.GoalTypes)
            //        if (_ruleManager.CanFactCaptureClass(conclusion, goalType))
            //        {
            //            resultFacts.Add(conclusion);
            //            break;
            //        }

            //    //var resultFacts = rule.Conclusions
            //    //    .Where(f => request.Problem.DesireTypes.Contains(f.Name))
            //    //    .ToList();

            //    if (resultFacts.Count <= 0) continue;

            //    var missingFacts = new List<Fact>(rule.Hypotheses);
            //    foreach (var hypothesis in rule.Hypotheses)
            //        if (_knownFacts.Contains(hypothesis)) missingFacts.Remove(hypothesis);

            //    var priority = rule.CertaintyFactor * (1 - (double) missingFacts.Count / rule.Hypotheses.Count);

            //    if (priority > 0)
            //        incompleteFacts.Add((priority, missingFacts, resultFacts));
            //}

            //if (incompleteFacts.Count == 0) return null;

            //return incompleteFacts
            //    .OrderByDescending(r => r.Item1)
            //    .ToList();
        }

        public IReadOnlyCollection<Relation> HighPriorityRelationRules { get; }
        public IReadOnlyCollection<Relation> LowPriorityRelationRules { get; }

        public IReadOnlyCollection<LogicRule> HighPriorityLogicRules =>
            new ReadOnlyCollection<LogicRule>(_highPriorityLogicRules);

        public IReadOnlyCollection<LogicRule> LowPriorityLogicRules =>
            new ReadOnlyCollection<LogicRule>(_lowPriorityLogicRules);

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

                foreach (var relationRule in HighPriorityRelationRules)
                {
                    foreach (var fact in _knownFacts)
                    {
                        if (!(fact is IndividualFact individualFact)) continue;
                        if (relationRule.AllDomains != null && relationRule.AllDomains.Contains(new Class(individualFact.Name)))
                        {
                            hasNewFacts = InferIndividualFact(individualFact, relationRule);
                            if (hasNewFacts) break;
                        }
                    }

                    if (hasNewFacts) break;
                }

                if (!hasNewFacts)
                    foreach (var logicRule in _highPriorityLogicRules)
                    {
                        hasNewFacts = InferLogicRule(logicRule);
                        if (hasNewFacts)
                        {
                            _highPriorityLogicRules.Remove(logicRule);
                            break;
                        }
                    }

                if (!hasNewFacts)
                    foreach (var relationRule in LowPriorityRelationRules)
                    {
                        foreach (var fact in _knownFacts)
                        {
                            if (!(fact is IndividualFact individualFact)) continue;
                            if (relationRule.AllDomains != null && relationRule.AllDomains.Contains(new Class(individualFact.Name)))
                            {
                                hasNewFacts = InferIndividualFact(individualFact, relationRule);
                                if (hasNewFacts) break;
                            }
                        }

                        if (hasNewFacts) break;
                    }

                if (!hasNewFacts)
                    foreach (var logicRule in _lowPriorityLogicRules)
                    {
                        hasNewFacts = InferLogicRule(logicRule);
                        if (hasNewFacts)
                        {
                            _lowPriorityLogicRules.Remove(logicRule);
                            break;
                        }
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
            throw new NotImplementedException();

            //var logicRules = SortRulesByRequest();

            //foreach (var rule in logicRules)
            //{
            //    var allHypothesesInKnown = true;
            //    foreach (var hypothesis in rule.Hypotheses)
            //        if (!Backtrack(hypothesis, 0))
            //        {
            //            allHypothesesInKnown = false;
            //            break;
            //        }
            //    if (allHypothesesInKnown)
            //        AddFactsToKnown(rule.Conclusions.ToArray());
            //}

            //var goalFacts = GetGoalFactsInKnown();
            //return goalFacts;
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

        private (IReadOnlyCollection<Relation> highPriorityRelationRules,
            IReadOnlyCollection<Relation> lowPriorityRelationRules,
            IList<LogicRule> highPriorityLogicRules,
            IList<LogicRule> lowPriorityLogicRules)
            SortRulesByRequest()
        {
            var relationRules = _ruleManager.RelationRules.ToList();
            var logicRules = _ruleManager.LogicRules.ToList();

            var highPriorityRelationRules = new List<Relation>();
            for (var i = 0; i < relationRules.Count;)
            {
                var hasGoalType = false;
                if (relationRules[i].AllRanges != null)
                    hasGoalType = _request.Problem.GoalTypes.Any(g => relationRules[i].AllRanges.Contains(g));

                var hasSuggestType = false;
                if (hasGoalType && relationRules[i].AllDomains != null)
                    hasSuggestType = _request.Problem.SuggestTypes.Any(s => relationRules[i].AllDomains.Contains(s));

                if (hasGoalType && hasSuggestType)
                {
                    highPriorityRelationRules.Add(relationRules[i]);
                    relationRules.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
            var lowPriorityRelationRules = relationRules;

            var highPriorityLogicRules = new List<LogicRule>();
            for (var i = 0; i < logicRules.Count;)
                if (logicRules[i].Problems != null && logicRules[i].Problems.Contains(_request.Problem))
                {
                    highPriorityLogicRules.Add(logicRules[i]);
                    logicRules.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            highPriorityLogicRules = highPriorityLogicRules.OrderByDescending(r => r.CertaintyFactor).ToList();
            var lowPriorityLogicRules = logicRules.OrderByDescending(r => r.CertaintyFactor).ToList();

            return (highPriorityRelationRules, lowPriorityRelationRules, highPriorityLogicRules, lowPriorityLogicRules);
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

        private bool InferIndividualFact([NotNull] IndividualFact individualFact, [NotNull] Relation relation)
        {
            Check.NotNull(individualFact, nameof(individualFact));
            Check.NotNull(relation, nameof(relation));

            if (_inferredRelationalRules.Any(r => r.Key.Equals(individualFact) && r.Value.Equals(relation)))
                return false;

            _inferredRelationalRules.Add(new KeyValuePair<IndividualFact, Relation>(individualFact, relation));

            var hasNewFacts = false;
            var relationValue = _ontologyManager.GetRelationValue(individualFact.Individual, relation.Id);
            if (relationValue != null)
                foreach (var individual in relationValue)
                {
                    var directClass = individual.DirectClass;
                    if (directClass != null)
                    {
                        var newIndividualFact = new IndividualFact(directClass.Id, individual.Id);

                        if (AddFactsToKnown(newIndividualFact) > 0)
                            hasNewFacts = true;
                    }
                }

            return hasNewFacts;
        }
    }
}