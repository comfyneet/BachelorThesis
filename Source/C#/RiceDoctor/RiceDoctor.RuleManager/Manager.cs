using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;
using JsonConvert = RiceDoctor.Shared.JsonConvert;

namespace RiceDoctor.RuleManager
{
    public class Manager : IRuleManager
    {
        private readonly IOntologyManager _ontologyManager = OntologyManager.Manager.Instance;

        [JsonConstructor]
        public Manager(
            [NotNull] IReadOnlyList<Problem> problems,
            [NotNull] IReadOnlyCollection<LogicRule> rules)
        {
            Check.NotNull(problems, nameof(problems));
            Check.NotNull(rules, nameof(rules));

            Problems = problems;
            Rules = rules;
        }

        public Manager([NotNull] string problemData, [NotNull] string ruleData)
        {
            Check.NotEmpty(problemData, nameof(problemData));
            Check.NotEmpty(ruleData, nameof(ruleData));

            Problems = JsonTemplates.JsonProblem.Deserialize(problemData, _ontologyManager);

            var lexer = new RuleLexer(ruleData);
            var parser = new RuleParser(lexer);
            Rules = parser.Parse();
        }

        public IReadOnlyList<Problem> Problems { get; }

        public IReadOnlyCollection<Rule> Rules { get; }

        public bool CanClassCaptureFact(Class type, Fact fact)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(fact, nameof(fact));

            if (fact.Name == type.Id) return true;

            if (type.GetAllSubClasses() != null && type.GetAllSubClasses().Any(sc => sc.Id == fact.Name)) return true;

            return false;
        }

        private class JsonTemplates
        {
            public class JsonProblem
            {
                public string Type { get; set; }
                public List<string> GoalTypes { get; set; }
                public List<string> SuggestTypes { get; set; }
                public bool SuggestFuzzyTypes { get; set; }

                [NotNull]
                public static IReadOnlyList<Problem> Deserialize(
                    [NotNull] string json,
                    [NotNull] IOntologyManager ontologyManager)
                {
                    Check.NotEmpty(json, nameof(json));
                    Check.NotNull(ontologyManager, nameof(ontologyManager));

                    var templateProblems = JsonConvert.Deserialize<List<JsonProblem>>(json);

                    var problems = new List<Problem>();
                    var allTypes = new Dictionary<string, Class>();
                    foreach (var templateProblem in templateProblems)
                    {
                        var goalTypes = new List<Class>();
                        foreach (var templateGoalType in templateProblem.GoalTypes)
                        {
                            if (!allTypes.TryGetValue(templateGoalType, out Class goalType))
                            {
                                goalType = ontologyManager.GetClass(templateGoalType);
                                if (goalType == null)
                                    throw new ArgumentException(CoreStrings.NonexistentType(templateGoalType));
                                allTypes.Add(templateGoalType, goalType);
                            }
                            goalTypes.Add(goalType);
                        }

                        var suggestTypes = new List<Class>();
                        foreach (var templateSuggestType in templateProblem.SuggestTypes)
                        {
                            if (!allTypes.TryGetValue(templateSuggestType, out Class suggestType))
                            {
                                suggestType = ontologyManager.GetClass(templateSuggestType);
                                if (suggestType == null)
                                    throw new ArgumentException(CoreStrings.NonexistentType(templateSuggestType));
                                allTypes.Add(templateSuggestType, suggestType);
                            }
                            suggestTypes.Add(suggestType);
                        }

                        problems.Add(new Problem(
                            templateProblem.Type,
                            goalTypes,
                            suggestTypes,
                            templateProblem.SuggestFuzzyTypes));
                    }

                    return problems;
                }
            }
        }
    }
}