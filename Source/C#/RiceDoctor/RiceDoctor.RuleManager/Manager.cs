using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class Manager : IRuleManager
    {
        private readonly IOntologyManager _ontologyManager = OntologyManager.Manager.Instance;

        public Manager([NotNull] string problemData, [NotNull] string logicData, [NotNull] string relationData)
        {
            Check.NotEmpty(problemData, nameof(problemData));
            Check.NotEmpty(logicData, nameof(logicData));
            Check.NotEmpty(relationData, nameof(relationData));

            (Problems, LogicRules) = MakeLogicRules(problemData, logicData);

            RelationRules = relationData
                .Split(new[] {"\r\n", "\n"}, StringSplitOptions.None)
                .Distinct()
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyCollection<Problem> Problems { get; }

        public IReadOnlyCollection<LogicRule> LogicRules { get; }

        public IReadOnlyCollection<string> RelationRules { get; }

        public bool CanFactCaptureClass(Fact fact, Class type)
        {
            Check.NotNull(fact, nameof(fact));
            Check.NotNull(type, nameof(type));

            if (fact.Name == type.Id) return true;

            if (type.AllSuperClasses != null && type.AllSuperClasses.Any(sc => sc.Id == fact.Name)) return true;

            if (type.AllSubClasses != null && type.AllSubClasses.Any(sc => sc.Id == fact.Name)) return true;

            return false;
        }

        private (IReadOnlyCollection<Problem>, IReadOnlyCollection<LogicRule>) MakeLogicRules(string problemData,
            string logicData)
        {
            var lexer = new LogicLexer(logicData);
            var parser = new LogicParser(lexer);
            var rules = parser.Parse();
            var problems = JsonTemplates.JsonProblemRoot.Deserialize(problemData, _ontologyManager);

            foreach (var rule in rules)
            {
                List<Problem> tmpProblems = null;
                foreach (var problem in problems)
                    if (CanRuleHaveProblem(rule, problem))
                    {
                        if (tmpProblems == null) tmpProblems = new List<Problem>();
                        tmpProblems.Add(problem);
                    }
                rule.Problems = tmpProblems;
            }

            return (problems, rules);
        }

        private bool CanRuleHaveProblem(LogicRule rule, Problem problem)
        {
            var hasDesireType = false;
            foreach (var desireType in problem.DesireTypes)
            {
                hasDesireType = rule.Conclusions.Any(fact => CanFactCaptureClass(fact, desireType));
                if (hasDesireType) break;
            }

            if (!hasDesireType) return false;

            var hasSuggestType = false;
            foreach (var suggestType in problem.SuggestTypes)
            {
                hasSuggestType = rule.Hypotheses.Any(fact => CanFactCaptureClass(fact, suggestType));
                if (hasSuggestType) break;
            }

            return hasSuggestType;
        }

        private class JsonTemplates
        {
            public class JsonProblem
            {
                public string Type { get; set; }
                public List<string> DesireFactTypes { get; set; }
                public List<string> SuggestFactTypes { get; set; }
            }

            public class JsonProblemRoot
            {
                public List<JsonProblem> Problems { get; set; }

                [NotNull]
                public static IReadOnlyCollection<Problem> Deserialize([NotNull] string jsonProblems,
                    [NotNull] IOntologyManager ontologyManager)
                {
                    Check.NotEmpty(jsonProblems, nameof(jsonProblems));
                    Check.NotNull(ontologyManager, nameof(ontologyManager));

                    var templateProblems = JsonConvert.Deserialize<JsonProblemRoot>(jsonProblems);

                    var problems = new List<Problem>();
                    var allTypes = new Dictionary<string, Class>();
                    foreach (var templateProblem in templateProblems.Problems)
                    {
                        var desireTypes = new List<Class>();
                        foreach (var templateDesireType in templateProblem.DesireFactTypes)
                        {
                            Class desireType;
                            if (!allTypes.TryGetValue(templateDesireType, out desireType))
                            {
                                desireType = ontologyManager.GetClass(templateDesireType);
                                if (desireType == null) throw new ArgumentException("Type doesn't exist.");
                                allTypes.Add(templateDesireType, desireType);
                            }
                            desireTypes.Add(desireType);
                        }

                        var suggestTypes = new List<Class>();
                        foreach (var templateSuggestType in templateProblem.SuggestFactTypes)
                        {
                            Class suggestType;
                            if (!allTypes.TryGetValue(templateSuggestType, out suggestType))
                            {
                                suggestType = ontologyManager.GetClass(templateSuggestType);
                                if (suggestType == null) throw new ArgumentException("Type doesn't exist.");
                                allTypes.Add(templateSuggestType, suggestType);
                            }
                            suggestTypes.Add(suggestType);
                        }

                        problems.Add(new Problem(templateProblem.Type, desireTypes, suggestTypes));
                    }

                    return problems.AsReadOnly();
                }
            }
        }
    }
}