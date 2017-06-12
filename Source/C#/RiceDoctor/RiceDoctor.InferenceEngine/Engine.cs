using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using static RiceDoctor.InferenceEngine.ResponseType;

namespace RiceDoctor.InferenceEngine
{
    public class Engine : IInferenceEngine
    {
        [NotNull] private readonly List<LogicRule> _inferredLogicRules;
        [NotNull] private readonly List<KeyValuePair<IndividualFact, Relation>> _inferredRelationRules;
        [NotNull] [JsonProperty] private readonly IList<Fact> _knownFacts;
        [NotNull] [JsonProperty] private readonly Request _request;
        [NotNull] [JsonProperty] private readonly IList<Fact> _unknownFacts;
        [CanBeNull] [JsonProperty] private List<Fact> _goalFacts;
        [NotNull] private List<LogicRule> _highPriorityLogicRules;
        [NotNull] private List<Relation> _highPriorityRelationRules;
        [NotNull] private List<LogicRule> _lowPriorityLogicRules;
        [NotNull] private List<Relation> _lowPriorityRelationRules;
        [NotNull] private List<LogicRule> _midPriorityLogicRules;
        [NotNull] private List<Relation> _midPriorityRelationRules;
        [NotNull] public IOntologyManager _ontologyManager;
        [NotNull] public IRuleManager _ruleManager;
        [CanBeNull] [JsonProperty] private int? _totalRemainingGuessableFacts;

        [JsonConstructor]
        public Engine(
            [NotNull] IList<Fact> _knownFacts,
            [NotNull] Request _request,
            [NotNull] IList<Fact> _unknownFacts,
            [CanBeNull] List<Fact> _goalFacts,
            [CanBeNull] int? _totalRemainingGuessableFacts,
            [NotNull] List<LogicRule> highPriorityLogicRules,
            [NotNull] List<Relation> highPriorityRelationRules,
            [NotNull] List<LogicRule> lowPriorityLogicRules,
            [NotNull] List<Relation> lowPriorityRelationRules,
            [NotNull] List<LogicRule> midPriorityLogicRules,
            [NotNull] List<Relation> midPriorityRelationRules,
            [NotNull] List<LogicRule> inferredLogicRules,
            [NotNull] List<KeyValuePair<IndividualFact, Relation>> inferredRelationRules)
        {
            Check.NotNull(_knownFacts, nameof(_knownFacts));
            Check.NotNull(_request, nameof(_request));
            Check.NotNull(_unknownFacts, nameof(_unknownFacts));
            Check.NotNull(highPriorityLogicRules, nameof(highPriorityLogicRules));
            Check.NotNull(highPriorityRelationRules, nameof(highPriorityRelationRules));
            Check.NotNull(lowPriorityLogicRules, nameof(lowPriorityLogicRules));
            Check.NotNull(lowPriorityRelationRules, nameof(lowPriorityRelationRules));
            Check.NotNull(midPriorityLogicRules, nameof(midPriorityLogicRules));
            Check.NotNull(midPriorityRelationRules, nameof(midPriorityRelationRules));
            Check.NotNull(inferredLogicRules, nameof(inferredLogicRules));
            Check.NotNull(inferredRelationRules, nameof(inferredRelationRules));

            this._knownFacts = _knownFacts;
            this._request = _request;
            this._unknownFacts = _unknownFacts;
            this._goalFacts = _goalFacts;
            this._totalRemainingGuessableFacts = _totalRemainingGuessableFacts;
            _highPriorityLogicRules = highPriorityLogicRules;
            _highPriorityRelationRules = highPriorityRelationRules;
            _lowPriorityLogicRules = lowPriorityLogicRules;
            _lowPriorityRelationRules = lowPriorityRelationRules;
            _midPriorityLogicRules = midPriorityLogicRules;
            _midPriorityRelationRules = midPriorityRelationRules;
            _inferredLogicRules = inferredLogicRules;
            _inferredRelationRules = inferredRelationRules;
        }

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
            _inferredRelationRules = new List<KeyValuePair<IndividualFact, Relation>>();
            _knownFacts = new List<Fact>();
        }

        public void HandleGuessableFacts(IReadOnlyCollection<Tuple<Fact, bool?>> guessableFacts)
        {
            Check.NotNull(guessableFacts, nameof(guessableFacts));

            foreach (var guessableFact in guessableFacts)
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
                return new Response(forwardResults, InferredResults);

            try
            {
                var backwardResults = InferBackwardChaining();
                if (backwardResults != null)
                    return new Response(backwardResults, InferredResults);
            }
            catch (GuessableFactException e)
            {
                return new Response(e.Facts, GuessableFacts);
            }

            return new Response();
        }

        public IReadOnlyCollection<Tuple<double, LogicRule, IReadOnlyList<Fact>>> GetIncompleteRules()
        {
            var incompleteRules = new List<Tuple<double, LogicRule, IReadOnlyList<Fact>>>();
            var inferableRules = _highPriorityLogicRules.ToList();
            inferableRules.AddRange(_midPriorityLogicRules);
            foreach (var rule in inferableRules)
            {
                if (!rule.Hypotheses.Any(IsFactInKnown)) continue;

                var goalFacts = rule.Conclusions
                    .Where(c => _request.Problem.GoalTypes.Any(g => _ruleManager.CanClassCaptureFact(g, c)))
                    .ToList();

                if (goalFacts.Count == 0) continue;

                var missingFacts = rule.Hypotheses
                    .Where(h => !IsFactInKnown(h) &&
                                _request.Problem.SuggestTypes.Any(s => _ruleManager.CanClassCaptureFact(s, h)))
                    .ToList();

                var priority = rule.CertaintyFactor * (1 - (double) missingFacts.Count / rule.Hypotheses.Count);

                if (priority > 0)
                    incompleteRules.Add(new Tuple<double, LogicRule, IReadOnlyList<Fact>>(priority, rule, goalFacts));
            }

            return incompleteRules
                .OrderByDescending(r => r.Item1)
                .ToList();
        }

        public IReadOnlyCollection<Relation> HighPriorityRelationRules => _highPriorityRelationRules;

        public IReadOnlyCollection<Relation> MidPriorityRelationRules => _midPriorityRelationRules;

        public IReadOnlyCollection<Relation> LowPriorityRelationRules => _lowPriorityRelationRules;

        public IReadOnlyCollection<LogicRule> HighPriorityLogicRules => _highPriorityLogicRules;

        public IReadOnlyCollection<LogicRule> MidPriorityLogicRules => _midPriorityLogicRules;

        public IReadOnlyCollection<LogicRule> LowPriorityLogicRules => _lowPriorityLogicRules;

        public IReadOnlyCollection<LogicRule> InferredLogicRules => _inferredLogicRules;

        public IReadOnlyCollection<KeyValuePair<IndividualFact, Relation>> InferredRelationRules =>
            _inferredRelationRules;

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

                foreach (var relationRule in _highPriorityRelationRules)
                {
                    foreach (var fact in _knownFacts)
                    {
                        if (!(fact is IndividualFact individualFact)) continue;
                        if (relationRule.GetAllDomains() != null &&
                            relationRule.GetAllDomains().Any(d => d.Id == individualFact.Name))
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

                foreach (var relationRule in _midPriorityRelationRules)
                {
                    foreach (var fact in _knownFacts)
                    {
                        if (!(fact is IndividualFact individualFact)) continue;
                        if (relationRule.GetAllDomains() != null &&
                            relationRule.GetAllDomains().Any(d => d.Id == individualFact.Name))
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

                foreach (var relationRule in _lowPriorityRelationRules)
                {
                    foreach (var fact in _knownFacts)
                    {
                        if (!(fact is IndividualFact individualFact)) continue;
                        if (relationRule.GetAllDomains() != null &&
                            relationRule.GetAllDomains().Any(d => d.Id == individualFact.Name))
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
        private IReadOnlyCollection<Fact> InferBackwardChaining(int totalEstimatedGuessableFacts = 4)
        {
            if (_totalRemainingGuessableFacts == null) _totalRemainingGuessableFacts = 25;
            else if (_totalRemainingGuessableFacts <= 0) return _goalFacts;

            var inferableRules = _highPriorityLogicRules.ToList();
            inferableRules.AddRange(_midPriorityLogicRules);

            var lowInferableRules = new List<LogicRule>();
            foreach (var unknownFact in _unknownFacts)
            {
                var tmpLowInferableRules = _lowPriorityLogicRules
                    .Where(rule => rule.Conclusions.Any(c => c.Equals(unknownFact)))
                    .ToList();
                lowInferableRules.AddRange(tmpLowInferableRules);
            }
            lowInferableRules = lowInferableRules.Distinct()
                .OrderBy(rule => rule.CertaintyFactor)
                .ToList();
            inferableRules.AddRange(lowInferableRules);

            var guessableFacts = new List<Fact>();
            foreach (var rule in inferableRules)
            {
                var hasNewFacts = InferLogicRule(rule);
                if (hasNewFacts) break;

                guessableFacts.AddRange(rule.Hypotheses.Where(f => !IsFactInKnown(f) && !_unknownFacts.Contains(f)));
                guessableFacts = guessableFacts.Distinct().ToList();

                _totalRemainingGuessableFacts -= guessableFacts.Count;
                if (guessableFacts.Count >= totalEstimatedGuessableFacts) break;
            }

            if (guessableFacts.Count != 0) throw new GuessableFactException(guessableFacts);

            return _goalFacts;
        }

        private void SortRulesByRequest()
        {
            var relationRules = _ruleManager.RelationRules.ToList();
            var logicRules = _ruleManager.LogicRules.ToList();

            _highPriorityRelationRules = new List<Relation>();
            _midPriorityRelationRules = new List<Relation>();
            for (var i = 0; i < relationRules.Count;)
            {
                var hasGoalType = false;
                if (relationRules[i].GetAllRanges() != null)
                    hasGoalType = _request.Problem.GoalTypes.Any(g => relationRules[i].GetAllRanges().Contains(g));

                var hasSuggestType = false;
                if (hasGoalType && relationRules[i].GetAllDomains() != null)
                    hasSuggestType =
                        _request.Problem.SuggestTypes.Any(s => relationRules[i].GetAllDomains().Contains(s));

                if (hasGoalType)
                {
                    if (hasSuggestType) _highPriorityRelationRules.Add(relationRules[i]);
                    else _midPriorityRelationRules.Add(relationRules[i]);
                    relationRules.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
            _lowPriorityRelationRules = relationRules;

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

            if (_inferredRelationRules.Any(r => r.Key.Equals(individualFact) && r.Value.Equals(relation)))
                return false;

            _inferredRelationRules.Add(new KeyValuePair<IndividualFact, Relation>(individualFact, relation));

            var hasNewFacts = false;
            var relationValue = _ontologyManager.GetRelationValue(individualFact.Value, relation.Id);
            if (relationValue != null)
                foreach (var individual in relationValue)
                {
                    var directClass = individual.GetDirectClass();
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