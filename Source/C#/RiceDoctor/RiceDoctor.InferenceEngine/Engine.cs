using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RiceDoctor.FuzzyManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using static RiceDoctor.InferenceEngine.ResponseType;

namespace RiceDoctor.InferenceEngine
{
    public class Engine : IInferenceEngine
    {
        [NotNull] private readonly List<LogicRule> _inferredLogicRules;
        [NotNull] private readonly List<KeyValuePair<IndividualFact, RelationRule>> _inferredRelationRules;
        [NotNull] [JsonProperty] private readonly IList<Fact> _knownFacts;
        [NotNull] [JsonProperty] private readonly Request _request;
        [NotNull] private readonly List<Rule> _rules;
        [NotNull] [JsonProperty] private readonly IList<Fact> _unknownFacts;
        [NotNull] public IFuzzyManager _fuzzyManager;
        [CanBeNull] [JsonProperty] private List<Fact> _goalFacts;
        [CanBeNull] [JsonProperty] private int? _maxGuessableFactsCanBeAsked;
        [NotNull] public IOntologyManager _ontologyManager;
        [NotNull] public IRuleManager _ruleManager;

        [JsonConstructor]
        public Engine(
            [NotNull] IList<Fact> _knownFacts,
            [NotNull] Request _request,
            [NotNull] IList<Fact> _unknownFacts,
            [CanBeNull] List<Fact> _goalFacts,
            [CanBeNull] int? _maxGuessableFactsCanBeAsked,
            [NotNull] List<Rule> rules,
            [NotNull] List<LogicRule> inferredLogicRules,
            [NotNull] List<KeyValuePair<IndividualFact, RelationRule>> inferredRelationRules)
        {
            Check.NotNull(_knownFacts, nameof(_knownFacts));
            Check.NotNull(_request, nameof(_request));
            Check.NotNull(_unknownFacts, nameof(_unknownFacts));
            Check.NotNull(rules, nameof(rules));
            Check.NotNull(inferredLogicRules, nameof(inferredLogicRules));
            Check.NotNull(inferredRelationRules, nameof(inferredRelationRules));

            this._knownFacts = _knownFacts;
            this._request = _request;
            this._unknownFacts = _unknownFacts;
            this._goalFacts = _goalFacts;
            this._maxGuessableFactsCanBeAsked = _maxGuessableFactsCanBeAsked;
            _rules = rules;
            _inferredLogicRules = inferredLogicRules;
            _inferredRelationRules = inferredRelationRules;
        }

        public Engine(
            [NotNull] IRuleManager ruleManager,
            [NotNull] IOntologyManager ontologyManager,
            [NotNull] IFuzzyManager fuzzyManager,
            [NotNull] Request request)
        {
            Check.NotNull(ruleManager, nameof(ruleManager));
            Check.NotNull(ontologyManager, nameof(ontologyManager));
            Check.NotNull(fuzzyManager, nameof(fuzzyManager));
            Check.NotNull(request, nameof(request));

            _ruleManager = ruleManager;
            _ontologyManager = ontologyManager;
            _fuzzyManager = fuzzyManager;
            _request = request;

            _rules = SortRulesByRequest();

            _unknownFacts = new List<Fact>();
            _inferredLogicRules = new List<LogicRule>();
            _inferredRelationRules = new List<KeyValuePair<IndividualFact, RelationRule>>();
            _knownFacts = new List<Fact>();
        }

        public void HandleGuessableFacts(IReadOnlyCollection<Tuple<Fact, bool?>> guessableFacts)
        {
            Check.NotNull(guessableFacts, nameof(guessableFacts));

            foreach (var guessableFact in guessableFacts)
                if (guessableFact.Item2 == true)
                    AddFactsToKnown(guessableFact.Item1);
                else if (guessableFact.Item2 == false)
                    for (var i = 0; i < _rules.Count;)
                    {
                        var rule = _rules[i] as LogicRule;
                        if (rule != null && (rule.Hypotheses.Contains(guessableFact.Item1) ||
                                             rule.Conclusions.Contains(guessableFact.Item1)))
                            _rules.RemoveAt(i);
                        else ++i;
                    }
                else
                    _unknownFacts.Add(guessableFact.Item1);
        }

        public Response Infer()
        {
            var forwardResults = InferMixedForwardChaining();
            if (forwardResults != null)
                return new Response(forwardResults, InferredResults);

            try
            {
                var backwardResults = InferBackward();
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

            foreach (var rule in _rules.OfType<LogicRule>())
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

        public IReadOnlyCollection<Rule> Rules => _rules;

        public IReadOnlyCollection<LogicRule> InferredLogicRules => _inferredLogicRules;

        public IReadOnlyCollection<KeyValuePair<IndividualFact, RelationRule>> InferredRelationRules =>
            _inferredRelationRules;

        public int AddFactsToKnown(params Fact[] facts)
        {
            Check.NotEmpty(facts, nameof(facts));

            var count = 0;
            var individualFacts = new List<IndividualFact>();
            foreach (var fact in facts)
                if (fact is FuzzyFact fuzzyFact)
                    foreach (var variable in _fuzzyManager.Variables)
                    {
                        if (variable.Id != fuzzyFact.Name) continue;
                        var newTerms = variable.Stmt.Execute(fuzzyFact.NumberValue);
                        foreach (var newTerm in newTerms)
                            if (newTerm.Value > 0) individualFacts.Add(new IndividualFact(variable.Id, newTerm.Key));
                    }
                else individualFacts.Add((IndividualFact) fact);

            foreach (var fact in individualFacts)
                if (!_knownFacts.Any(f => f.Equals(fact)))
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
        private IReadOnlyCollection<Fact> InferMixedForwardChaining(int minInferredGoals = 5)
        {
            while (true)
            {
                var hasNewFacts = false;

                foreach (var rule in _rules)
                {
                    if (rule is RelationRule relationRule)
                    {
                        foreach (var fact in _knownFacts)
                        {
                            if (!(fact is IndividualFact individualFact)) continue;
                            if (relationRule.InferredDomains.Any(d => d == individualFact.Name))
                            {
                                hasNewFacts = InferIndividualFact(individualFact, relationRule);
                                if (hasNewFacts) break;
                            }
                        }
                    }
                    else
                    {
                        var logicRule = (LogicRule) rule;
                        hasNewFacts = InferLogicRule(logicRule);
                        if (hasNewFacts)
                        {
                            _rules.Remove(logicRule);
                            break;
                        }
                    }

                    if (!hasNewFacts) continue;
                    if (_goalFacts?.Count >= minInferredGoals) break;
                }

                break;
            }

            return _goalFacts;
        }

        [CanBeNull]
        private IReadOnlyCollection<Fact> InferBackward(int minGuessableFactsToAskEachTime = 4)
        {
            if (_maxGuessableFactsCanBeAsked == null) _maxGuessableFactsCanBeAsked = 25;
            else if (_maxGuessableFactsCanBeAsked <= 0) return _goalFacts;

            var guessableFacts = new List<Fact>();
            foreach (var rule in _rules.OfType<LogicRule>())
            {
                var hasNewFacts = InferLogicRule(rule);
                if (hasNewFacts) break;

                guessableFacts.AddRange(rule.Hypotheses.Where(f => !IsFactInKnown(f) && !_unknownFacts.Contains(f)));
                guessableFacts = guessableFacts.Distinct().ToList();

                _maxGuessableFactsCanBeAsked -= guessableFacts.Count;
                if (guessableFacts.Count >= minGuessableFactsToAskEachTime) break;
            }

            if (guessableFacts.Count != 0) throw new GuessableFactException(guessableFacts);

            return _goalFacts;
        }

        [NotNull]
        private List<Rule> SortRulesByRequest()
        {
            var relationRules = _ruleManager.Rules.OfType<RelationRule>().ToList();
            var logicRules = _ruleManager.Rules.OfType<LogicRule>().ToList();

            var highPriorityRelationRules = new List<RelationRule>();
            var midPriorityRelationRules = new List<RelationRule>();
            for (var i = 0; i < relationRules.Count;)
            {
                var hasGoalType = _request.Problem.GoalTypes.Any(c => relationRules[i].InferredRanges.Contains(c.Id));
                var hasSuggestType =
                    _request.Problem.SuggestTypes.Any(c => relationRules[i].InferredDomains.Contains(c.Id));

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
            var lowPriorityRelationRules = relationRules;

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
            highPriorityLogicRules = highPriorityLogicRules.OrderByDescending(r => r.CertaintyFactor).ToList();
            midPriorityLogicRules = midPriorityLogicRules.OrderByDescending(r => r.CertaintyFactor).ToList();
            var lowPriorityLogicRules = logicRules.OrderByDescending(r => r.CertaintyFactor).ToList();

            var rules = new List<Rule>();
            rules.AddRange(highPriorityRelationRules);
            rules.AddRange(highPriorityLogicRules);
            rules.AddRange(midPriorityRelationRules);
            rules.AddRange(midPriorityLogicRules);
            rules.AddRange(lowPriorityRelationRules);
            rules.AddRange(lowPriorityLogicRules);

            return rules;
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

        private bool InferIndividualFact([NotNull] IndividualFact individualFact, [NotNull] RelationRule relationRule)
        {
            Check.NotNull(individualFact, nameof(individualFact));
            Check.NotNull(relationRule, nameof(relationRule));

            if (_inferredRelationRules.Any(r => r.Key.Equals(individualFact) && r.Value.Equals(relationRule)))
                return false;

            var hasNewFacts = false;
            foreach (var relation in relationRule.Relations)
            {
                var relationValue = _ontologyManager.GetRelationValue(individualFact.Value, relation);
                if (relationValue == null) continue;

                foreach (var individual in relationValue)
                {
                    var directClass = individual.GetDirectClass();
                    if (directClass == null) continue;

                    var newIndividualFact = new IndividualFact(directClass.Id, individual.Id);

                    if (AddFactsToKnown(newIndividualFact) > 0)
                        hasNewFacts = true;
                }
            }

            _inferredRelationRules.Add(new KeyValuePair<IndividualFact, RelationRule>(individualFact, relationRule));

            return hasNewFacts;
        }
    }
}