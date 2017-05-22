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

            Problems = JsonTemplates.JsonRoot.Deserialize(problemData, _ontologyManager);

            LogicRules = MakeLogicRules(logicData);

            RelationRules = MakeRelationRules(relationData);
        }

        public IReadOnlyList<Problem> Problems { get; }

        public IReadOnlyCollection<LogicRule> LogicRules { get; }

        public IReadOnlyCollection<Relation> RelationRules { get; }

        public bool CanClassCaptureFact(Class type, Fact fact)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(fact, nameof(fact));

            if (fact.Name == type.Id) return true;

            if (type.AllSubClasses != null && type.AllSubClasses.Any(sc => sc.Id == fact.Name)) return true;

            return false;
        }

        [NotNull]
        private IReadOnlyCollection<LogicRule> MakeLogicRules(string logicData)
        {
            var lexer = new LogicLexer(logicData);
            var parser = new LogicParser(lexer);
            var rules = parser.Parse();

            return rules;
        }

        [NotNull]
        private IReadOnlyCollection<Relation> MakeRelationRules(string relationData)
        {
            var relationRules = relationData
                .Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

            var relations = new HashSet<Relation>();
            foreach (var relationRule in relationRules)
            {
                var relation = _ontologyManager.GetRelation(relationRule);
                if (relation == null) throw new ArgumentException($"Relation rule '{relationRule}' doesn't exist");

                if (!relations.Contains(relation)) relations.Add(relation);

                var inverseRelation = relation.InverseRelation;
                if (inverseRelation != null && !relations.Contains(inverseRelation))
                    relations.Add(inverseRelation);
            }

            return relations;
        }

        private class JsonTemplates
        {
            public class JsonProblem
            {
                public string Type { get; set; }
                public List<string> GoalTypes { get; set; }
                public List<string> SuggestTypes { get; set; }
            }

            public class JsonRoot
            {
                public List<JsonProblem> Problems { get; set; }

                [NotNull]
                public static IReadOnlyList<Problem> Deserialize(
                    [NotNull] string json,
                    [NotNull] IOntologyManager ontologyManager)
                {
                    Check.NotEmpty(json, nameof(json));
                    Check.NotNull(ontologyManager, nameof(ontologyManager));

                    var templateProblems = JsonConvert.Deserialize<JsonRoot>(json);

                    var problems = new List<Problem>();
                    var allTypes = new Dictionary<string, Class>();
                    foreach (var templateProblem in templateProblems.Problems)
                    {
                        var goalTypes = new List<Class>();
                        foreach (var templateGoalType in templateProblem.GoalTypes)
                        {
                            if (!allTypes.TryGetValue(templateGoalType, out Class goalType))
                            {
                                goalType = ontologyManager.GetClass(templateGoalType);
                                if (goalType == null)
                                    throw new ArgumentException($"Type '{templateGoalType}' doesn't exist.");
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
                                    throw new ArgumentException($"Type '{templateSuggestType}' doesn't exist.");
                                allTypes.Add(templateSuggestType, suggestType);
                            }
                            suggestTypes.Add(suggestType);
                        }

                        problems.Add(new Problem(templateProblem.Type, goalTypes, suggestTypes));
                    }

                    return problems;
                }
            }
        }
    }
}