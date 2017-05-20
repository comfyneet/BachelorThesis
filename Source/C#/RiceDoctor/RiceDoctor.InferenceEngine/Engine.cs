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
        [NotNull] private readonly IList<LogicRule> _inferredLogicRules;
        [NotNull] private readonly IList<KeyValuePair<IndividualFact, Relation>> _inferredRelationalRules;
        [NotNull] private readonly IList<Fact> _knownFacts;
        [NotNull] private readonly IOntologyManager _ontologyManager;
        [NotNull] private readonly Request _request;
        [NotNull] private readonly IRuleManager _ruleManager;
        [NotNull] private readonly IList<Fact> _unknownFacts;
        [CanBeNull] private List<Fact> _goalFacts;
        [NotNull] private IList<LogicRule> _highPriorityLogicRules;
        [NotNull] private IList<LogicRule> _lowPriorityLogicRules;
        [NotNull] private IList<LogicRule> _midPriorityLogicRules;

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

            SortRulesByRequest();

            _unknownFacts = new List<Fact>();
            _inferredLogicRules = new List<LogicRule>();
            _inferredRelationalRules = new List<KeyValuePair<IndividualFact, Relation>>();
            _knownFacts = new List<Fact>();

            if (request.KnownFacts != null)
                AddFactsToKnown(request.KnownFacts.ToArray());
        }

        public void HandleGuessableFact(Tuple<Fact, bool?> guessableFact)
        {
            Check.NotNull(guessableFact, nameof(guessableFact));

            if (guessableFact.Item2 == true)
            {
                AddFactsToKnown(guessableFact.Item1);
            }
            else if (guessableFact.Item2 == false)
            {
                for (var i = 0; i < _highPriorityLogicRules.Count;)
                    if (_highPriorityLogicRules[i].Hypotheses.Contains(guessableFact.Item1) ||
                        _highPriorityLogicRules[i].Conclusions.Contains(guessableFact.Item1))
                        _highPriorityLogicRules.RemoveAt(i);
                    else ++i;

                for (var i = 0; i < _midPriorityLogicRules.Count;)
                    if (_midPriorityLogicRules[i].Hypotheses.Contains(guessableFact.Item1) ||
                        _midPriorityLogicRules[i].Conclusions.Contains(guessableFact.Item1))
                        _midPriorityLogicRules.RemoveAt(i);
                    else ++i;

                for (var i = 0; i < _lowPriorityLogicRules.Count;)
                    if (_lowPriorityLogicRules[i].Hypotheses.Contains(guessableFact.Item1) ||
                        _lowPriorityLogicRules[i].Conclusions.Contains(guessableFact.Item1))
                        _lowPriorityLogicRules.RemoveAt(i);
                    else ++i;
            }
            else
            {
                _unknownFacts.Add(guessableFact.Item1);
            }
        }

        public Response Infer()
        {
            var forwardResults = InferForwardChaining();
            if (forwardResults != null)
                return new Response(forwardResults);

            try
            {
                var backwardResults = InferBackwardChaining();
                if (backwardResults != null)
                    return new Response(backwardResults);
            }
            catch (GuessableFactException e)
            {
                return new Response(e.Fact);
            }

            return new Response();
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

        public IReadOnlyCollection<Relation> HighPriorityRelationRules { get; private set; }
        public IReadOnlyCollection<Relation> MidPriorityRelationRules { get; private set; }
        public IReadOnlyCollection<Relation> LowPriorityRelationRules { get; private set; }

        public IReadOnlyCollection<LogicRule> HighPriorityLogicRules =>
            new ReadOnlyCollection<LogicRule>(_highPriorityLogicRules);

        public IReadOnlyCollection<LogicRule> MidPriorityLogicRules =>
            new ReadOnlyCollection<LogicRule>(_midPriorityLogicRules);

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

                    foreach (var goalType in _request.Problem.GoalTypes)
                        if (_ruleManager.CanClassCaptureFact(goalType, fact))
                        {
                            if (_goalFacts == null) _goalFacts = new List<Fact>();
                            _goalFacts.Add(fact);
                            break;
                        }

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
                        if (relationRule.AllDomains != null &&
                            relationRule.AllDomains.Any(d => d.Id == individualFact.Name))
                        {
                            hasNewFacts = InferIndividualFact(individualFact, relationRule);
                            if (hasNewFacts) break;
                        }
                    }

                    if (hasNewFacts) break;
                }
                if (hasNewFacts)
                {
                    if (_request.TotalGoals != null && _goalFacts?.Count >= _request.TotalGoals) break;
                    continue;
                }

                foreach (var logicRule in _highPriorityLogicRules)
                {
                    hasNewFacts = InferLogicRule(logicRule);
                    if (hasNewFacts)
                    {
                        _highPriorityLogicRules.Remove(logicRule);
                        break;
                    }
                }
                if (hasNewFacts)
                {
                    if (_request.TotalGoals != null && _goalFacts?.Count >= _request.TotalGoals) break;
                    continue;
                }

                foreach (var relationRule in MidPriorityRelationRules)
                {
                    foreach (var fact in _knownFacts)
                    {
                        if (!(fact is IndividualFact individualFact)) continue;
                        if (relationRule.AllDomains != null &&
                            relationRule.AllDomains.Any(d => d.Id == individualFact.Name))
                        {
                            hasNewFacts = InferIndividualFact(individualFact, relationRule);
                            if (hasNewFacts) break;
                        }
                    }

                    if (hasNewFacts) break;
                }
                if (hasNewFacts)
                {
                    if (_request.TotalGoals != null && _goalFacts?.Count >= _request.TotalGoals) break;
                    continue;
                }

                foreach (var logicRule in _midPriorityLogicRules)
                {
                    hasNewFacts = InferLogicRule(logicRule);
                    if (hasNewFacts)
                    {
                        _highPriorityLogicRules.Remove(logicRule);
                        break;
                    }
                }
                if (hasNewFacts)
                {
                    if (_request.TotalGoals != null && _goalFacts?.Count >= _request.TotalGoals) break;
                    continue;
                }

                foreach (var relationRule in LowPriorityRelationRules)
                {
                    foreach (var fact in _knownFacts)
                    {
                        if (!(fact is IndividualFact individualFact)) continue;
                        if (relationRule.AllDomains != null &&
                            relationRule.AllDomains.Any(d => d.Id == individualFact.Name))
                        {
                            hasNewFacts = InferIndividualFact(individualFact, relationRule);
                            if (hasNewFacts) break;
                        }
                    }

                    if (hasNewFacts) break;
                }
                if (hasNewFacts)
                {
                    if (_request.TotalGoals != null && _goalFacts?.Count >= _request.TotalGoals) break;
                    continue;
                }

                foreach (var logicRule in _lowPriorityLogicRules)
                {
                    hasNewFacts = InferLogicRule(logicRule);
                    if (hasNewFacts)
                    {
                        _lowPriorityLogicRules.Remove(logicRule);
                        break;
                    }
                }
                if (hasNewFacts)
                {
                    if (_request.TotalGoals != null && _goalFacts?.Count >= _request.TotalGoals) break;
                    continue;
                }

                break;
            }

            return _goalFacts;
        }

        [CanBeNull]
        private IReadOnlyCollection<Fact> InferBackwardChaining()
        {
            var inferableRules = _highPriorityLogicRules.ToList();
            inferableRules.AddRange(_midPriorityLogicRules);
            foreach (var rule in inferableRules)
            {
                var allHypothesesInKnown = true;
                foreach (var hypothesis in rule.Hypotheses)
                    if (!Backtrack(hypothesis, 0))
                    {
                        allHypothesesInKnown = false;
                        break;
                    }
                if (allHypothesesInKnown)
                {
                    AddFactsToKnown(rule.Conclusions.ToArray());

                    break;
                }
            }

            return _goalFacts;
        }

        private bool Backtrack([NotNull] Fact goal, int level)
        {
            if (IsFactInKnown(goal)) return true;

            if (!_unknownFacts.Contains(goal))
                throw new GuessableFactException(goal);

            var allLogicRules = _highPriorityLogicRules.ToList();
            allLogicRules.AddRange(_midPriorityLogicRules);
            allLogicRules.AddRange(_lowPriorityLogicRules);
            var inferableRules = allLogicRules
                .Where(rule => rule.Conclusions.Any(c => c.Equals(goal)))
                .OrderBy(r => CountFactsInKnown(r.Hypotheses))
                .ToList();

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

        private void SortRulesByRequest()
        {
            var relationRules = _ruleManager.RelationRules.ToList();
            var logicRules = _ruleManager.LogicRules.ToList();

            var highPriorityRelationRules = new List<Relation>();
            var midPriorityRelationRules = new List<Relation>();
            for (var i = 0; i < relationRules.Count;)
            {
                var hasGoalType = false;
                if (relationRules[i].AllRanges != null)
                    hasGoalType = _request.Problem.GoalTypes.Any(g => relationRules[i].AllRanges.Contains(g));

                var hasSuggestType = false;
                if (hasGoalType && relationRules[i].AllDomains != null)
                    hasSuggestType = _request.Problem.SuggestTypes.Any(s => relationRules[i].AllDomains.Contains(s));

                if (hasGoalType)
                {
                    if (hasSuggestType) highPriorityRelationRules.Add(relationRules[i]);
                    else midPriorityRelationRules.Add(relationRules[i]);
                    relationRules.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
            HighPriorityRelationRules = highPriorityRelationRules;
            MidPriorityRelationRules = midPriorityRelationRules;
            LowPriorityRelationRules = relationRules;

            var highPriorityLogicRules = new List<LogicRule>();
            var midPriorityLogicRules = new List<LogicRule>();
            for (var i = 0; i < logicRules.Count;)
            {
                var hasGoalType = _request.Problem.GoalTypes
                    .Any(g => logicRules[i].Conclusions.Any(c => _ruleManager.CanClassCaptureFact(g, c)));
                var hasSuggestType = _request.Problem.SuggestTypes
                    .Any(s => logicRules[i].Hypotheses.Any(h => _ruleManager.CanClassCaptureFact(s, h)));

                if (hasGoalType)
                {
                    if (hasSuggestType) highPriorityLogicRules.Add(logicRules[i]);
                    else midPriorityLogicRules.Add(logicRules[i]);
                    logicRules.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
            _highPriorityLogicRules = highPriorityLogicRules.OrderByDescending(r => r.CertaintyFactor).ToList();
            _midPriorityLogicRules = midPriorityLogicRules.OrderByDescending(r => r.CertaintyFactor).ToList();
            _lowPriorityLogicRules = logicRules.OrderByDescending(r => r.CertaintyFactor).ToList();
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