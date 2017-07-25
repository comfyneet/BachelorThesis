using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RiceDoctor.FuzzyManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.InferenceEngine
{
    public class Engine : IInferenceEngine
    {
        [NotNull] [JsonProperty] private readonly List<KeyValuePair<IndividualFact, RelationRule>> _inferredRelations;
        [NotNull] [JsonProperty] private readonly List<LogicRule> _inferredRules;
        [NotNull] [JsonProperty] private readonly IList<Fact> _knownFacts;
        [NotNull] [JsonProperty] private readonly IList<Fact> _reliableKnownFacts;
        [NotNull] [JsonProperty] private readonly Request _request;
        [NotNull] [JsonProperty] private readonly List<Rule> _rules;
        [NotNull] [JsonProperty] private readonly IList<Fact> _unknownFacts;
        [JsonProperty] private bool _canAskOneTime;
        [NotNull] [JsonIgnore] public IFuzzyManager _fuzzyManager;
        [CanBeNull] [JsonProperty] private List<Fact> _goalFacts;
        [NotNull] [JsonIgnore] public IOntologyManager _ontologyManager;
        [NotNull] [JsonIgnore] public IRuleManager _ruleManager;

        [JsonConstructor]
        public Engine(
            bool _canAskOneTime,
            [NotNull] IList<Fact> _reliableKnownFacts,
            [NotNull] IList<Fact> _knownFacts,
            [NotNull] Request _request,
            [NotNull] IList<Fact> _unknownFacts,
            [CanBeNull] List<Fact> _goalFacts,
            [NotNull] List<Rule> rules,
            [NotNull] List<LogicRule> inferredRules,
            [NotNull] List<KeyValuePair<IndividualFact, RelationRule>> inferredRelations)
        {
            Check.NotNull(_reliableKnownFacts, nameof(_reliableKnownFacts));
            Check.NotNull(_knownFacts, nameof(_knownFacts));
            Check.NotNull(_request, nameof(_request));
            Check.NotNull(_unknownFacts, nameof(_unknownFacts));
            Check.NotNull(rules, nameof(rules));
            Check.NotNull(inferredRules, nameof(inferredRules));
            Check.NotNull(inferredRelations, nameof(inferredRelations));

            this._canAskOneTime = _canAskOneTime;
            this._reliableKnownFacts = _reliableKnownFacts;
            this._knownFacts = _knownFacts;
            this._request = _request;
            this._unknownFacts = _unknownFacts;
            this._goalFacts = _goalFacts;
            _rules = rules;
            _inferredRules = inferredRules;
            _inferredRelations = inferredRelations;
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

            _canAskOneTime = true;
            _unknownFacts = new List<Fact>();
            _inferredRules = new List<LogicRule>();
            _inferredRelations = new List<KeyValuePair<IndividualFact, RelationRule>>();
            _knownFacts = new List<Fact>();
            _reliableKnownFacts = new List<Fact>();
        }

        public void HandleGuessableFacts(IReadOnlyCollection<Tuple<Fact, bool?>> guessableFacts)
        {
            Check.NotNull(guessableFacts, nameof(guessableFacts));

            foreach (var guessableFact in guessableFacts)
                if (guessableFact.Item2 == true)
                    AddFactsToKnown(true, guessableFact.Item1);
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
            if (forwardResults != null) return Response.ShowInferredResults(forwardResults);

            if (_canAskOneTime)
            {
                _canAskOneTime = false;

                var guessableFacts = FindFactsToAsk();
                return Response.AskGuessableFacts(guessableFacts);
            }

            var incompleteResults = InferIncompleteRules();
            return Response.ShowIncompleteResults(incompleteResults);
        }

        public IReadOnlyCollection<Rule> Rules => _rules;

        public IReadOnlyCollection<LogicRule> InferredRules => _inferredRules;

        public IReadOnlyCollection<KeyValuePair<IndividualFact, RelationRule>> InferredRelations =>
            _inferredRelations;

        public int AddFactsToKnown(bool reliable, params Fact[] facts)
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
                    if (reliable) _reliableKnownFacts.Add(fact);

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
        private IReadOnlyDictionary<Fact, double> InferMixedForwardChaining(int minInferredGoals = 5)
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
                                hasNewFacts = InferGeneratedLogicRulesFromRelation(individualFact, relationRule);
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

            if (_goalFacts != null)
            {
                var results = new Dictionary<Fact, double>();
                var knownFacts = _reliableKnownFacts.ToList();
                foreach (var goalFact in _goalFacts)
                {
                    var pair = FindBestPathForFact(goalFact, knownFacts, _inferredRules);
                    results.Add(goalFact, pair.Item2);
                }

                return results;
            }

            return null;
        }

        [NotNull]
        private IReadOnlyDictionary<Fact, double> InferIncompleteRules()
        {
            var results = new Dictionary<Fact, double>();
            var knownFacts = _reliableKnownFacts.ToList();
            var priorityRules = GetMidAndHighPriorityLogicRules(_rules);

            foreach (var rule in priorityRules)
            {
                var hypothesesInKnown = CountFactsInKnown(rule.Hypotheses);
                if (hypothesesInKnown == 0) continue;

                var hypotheses = new List<Fact>();
                foreach (var h in rule.Hypotheses)
                    if (IsFactInKnown(h)) hypotheses.Add(h);

                var incompleteRule = new LogicRule(hypotheses, rule.Conclusions,
                    rule.CertaintyFactor * ((double) hypothesesInKnown / rule.Hypotheses.Count));

                var inferredRules = _inferredRules.Append(incompleteRule).ToList();

                foreach (var c in rule.Conclusions)
                {
                    if (!_request.Problem.GoalTypes.Any(goalType => _ruleManager.CanClassCaptureFact(goalType, c)))
                        continue;

                    var pair = FindBestPathForFact(c, knownFacts, inferredRules);
                    if (results.ContainsKey(c))
                    {
                        if (results[c] < pair.Item2) results[c] = pair.Item2;
                    }
                    else
                    {
                        results.Add(c, pair.Item2);
                    }
                }
            }

            return results;
        }

        [NotNull]
        private IReadOnlyCollection<Fact> FindFactsToAsk(int maxRulesToAsk = 3)
        {
            var guessableFacts = new List<Fact>();

            var askableRules = GetMidAndHighPriorityLogicRules(_rules)
                .OrderBy(r => (float) (r.Hypotheses.Count - CountFactsInKnown(r.Hypotheses)) / r.Hypotheses.Count)
                .Take(maxRulesToAsk)
                .ToList();
            foreach (var askableRule in askableRules)
                guessableFacts.AddRange(
                    askableRule.Hypotheses.Where(f => !IsFactInKnown(f) && !_unknownFacts.Contains(f)));

            guessableFacts = guessableFacts.Distinct().ToList();

            return guessableFacts;
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

            if (_inferredRules.Any(rule.Equals)) return false;

            if (CountFactsInKnown(rule.Hypotheses) != rule.Hypotheses.Count) return false;

            _inferredRules.Add(rule);

            return AddFactsToKnown(false, rule.Conclusions.ToArray()) > 0;
        }

        private bool InferGeneratedLogicRulesFromRelation(
            [NotNull] IndividualFact individualFact,
            [NotNull] RelationRule relationRule)
        {
            Check.NotNull(individualFact, nameof(individualFact));
            Check.NotNull(relationRule, nameof(relationRule));

            if (_inferredRelations.Any(r => r.Key.Equals(individualFact) && r.Value.Equals(relationRule)))
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
                    var rule = new LogicRule(new List<Fact> {individualFact}, new List<Fact> {newIndividualFact}, 1.0);
                    if (InferLogicRule(rule)) hasNewFacts = true;
                }
            }

            _inferredRelations.Add(new KeyValuePair<IndividualFact, RelationRule>(individualFact, relationRule));

            return hasNewFacts;
        }

        [CanBeNull]
        private IReadOnlyCollection<Path> FindPaths(
            Fact fact,
            IReadOnlyCollection<Fact> knownFacts,
            IReadOnlyCollection<LogicRule> rules,
            int level)
        {
            var paths = new List<Path>();

            var rulesCanInferFact = rules.Where(r => r.Conclusions.Contains(fact)).ToList();
            if (rulesCanInferFact.Count == 0) return null;
            foreach (var ruleCanInferFact in rulesCanInferFact)
            {
                var previousPaths = new List<Path>();
                var newRules = rules.ToList();
                newRules.Remove(ruleCanInferFact);

                foreach (var h in ruleCanInferFact.Hypotheses)
                {
                    if (knownFacts.Contains(h)) continue;

                    var tmpPreviousPaths = FindPaths(h, knownFacts, newRules, level + 1);
                    if (tmpPreviousPaths == null) Logger.Log("Previous paths are null.");
                    else previousPaths.AddRange(tmpPreviousPaths);
                }

                if (previousPaths.Count > 0)
                {
                    foreach (var previousPath in previousPaths)
                    {
                        var newKnown = previousPath.Known.ToList();
                        if (!newKnown.Contains(fact)) newKnown.Add(fact);
                        var path = new Path(newKnown, previousPath.Chains.Append(ruleCanInferFact).ToList());
                        paths.Add(path);
                    }
                }
                else
                {
                    var newKnown = knownFacts.ToList();
                    if (!newKnown.Contains(fact)) newKnown.Add(fact);
                    paths.Add(new Path(newKnown, new List<LogicRule> {ruleCanInferFact}));
                }
            }

            return paths;
        }

        [NotNull]
        private IReadOnlyCollection<LogicRule> GetMidAndHighPriorityLogicRules(
            [NotNull] IReadOnlyCollection<Rule> rules)
        {
            Check.NotNull(rules, nameof(rules));

            var priorityRules = new List<LogicRule>();
            foreach (var logicRule in rules.OfType<LogicRule>())
            {
                var hasGoalType = _request.Problem.GoalTypes.Any(g => logicRule.Conclusions
                    .Any(c => _ruleManager.CanClassCaptureFact(g, c)));
                if (hasGoalType) priorityRules.Add(logicRule);
            }

            return priorityRules;
        }

        private Tuple<Path, double> FindBestPathForFact(
            [NotNull] Fact fact,
            [NotNull] IReadOnlyCollection<Fact> knownFacts,
            [NotNull] IReadOnlyCollection<LogicRule> rules)
        {
            Path bestPath = null;
            var bestCertainty = 0.0;
            var paths = FindPaths(fact, knownFacts, rules, 0);
            foreach (var path in paths)
            {
                var certainty = path.Chains.Aggregate(1.0, (current, rule) => current * rule.CertaintyFactor);
                if (certainty > bestCertainty)
                {
                    bestCertainty = certainty;
                    bestPath = path;
                }
            }

            return new Tuple<Path, double>(bestPath, bestCertainty);
        }
    }
}