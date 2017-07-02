using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;
using static RiceDoctor.RuleManager.RuleTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.RuleManager
{
    public class RuleParser : Parser<IReadOnlyCollection<Rule>>
    {
        private readonly IOntologyManager _ontologyManager = OntologyManager.Manager.Instance;

        [CanBeNull] private IReadOnlyCollection<Rule> _rules;

        public RuleParser([NotNull] RuleLexer lexer) : base(lexer)
        {
        }

        [NotNull]
        public override IReadOnlyCollection<Rule> Parse()
        {
            if (_rules != null) return _rules;

            _rules = ParseInferenceRules();

            if (CurrentToken.Type != Eof)
                throw new InvalidOperationException(CoreStrings.SyntaxError(Eof.Name, CurrentToken.Type.Name));

            return _rules;
        }

        [NotNull]
        private IReadOnlyCollection<Rule> ParseInferenceRules()
        {
            var rules = new List<Rule>();

            while (CurrentToken.Type != Eof)
                if (CurrentToken.Type == Ident && Peek().Type == Arrow)
                {
                    var relationRule = ParseRelationRule();
                    if (relationRule != null) rules.Add(relationRule);
                }
                else
                {
                    rules.AddRange(ParseLogicRules());
                }

            return rules.Distinct().ToList();
        }

        [CanBeNull]
        private RelationRule ParseRelationRule()
        {
            var domainName = ParseIdentifier();

            Eat(Arrow);

            var rangeName = ParseIdentifier();

            Eat(Semi);

            var domainClass = _ontologyManager.GetClass(domainName);
            var rangeClass = _ontologyManager.GetClass(rangeName);
            if (domainClass == null || rangeClass == null) return null;

            var ruleRelations = new List<string>();
            var relations = _ontologyManager.GetRelations();
            foreach (var relation in relations)
            {
                var allDomains = relation.GetAllDomains();
                var allRanges = relation.GetAllRanges();
                if (allDomains == null || allRanges == null) continue;

                if (allDomains.Any(d => d.Id == domainName) && allRanges.Any(r => r.Id == rangeName))
                    ruleRelations.Add(relation.Id);
            }

            if (ruleRelations.Count == 0) return null;

            var inferredDomains = new List<string> {domainName};
            var domainSubClasses = domainClass.GetAllSubClasses();
            if (domainSubClasses != null) inferredDomains.AddRange(domainSubClasses.Select(c => c.Id));

            var inferredRanges = new List<string> {rangeName};
            var rangeSubClasses = rangeClass.GetAllSubClasses();
            if (rangeSubClasses != null) inferredRanges.AddRange(rangeSubClasses.Select(c => c.Id));

            var rule = new RelationRule(domainName, rangeName, inferredDomains, inferredRanges, ruleRelations);

            return rule;
        }

        [NotNull]
        private IReadOnlyCollection<LogicRule> ParseLogicRules()
        {
            var hypotheses = ParseHypothese();

            Eat(Arrow);

            var conclusions = ParseConclusions();

            var certaintyFactor = 0.6;
            if (CurrentToken.Type == LBrace)
            {
                Eat(LBrace);

                certaintyFactor = ParseNumber();
                if (certaintyFactor < 0 || certaintyFactor > 1)
                    throw new InvalidOperationException(
                        $"The certainty factor '{certaintyFactor}' should be in between 0 and 1.");

                Eat(RBrace);
            }

            Eat(Semi);

            var logicRules = hypotheses
                .Select(h => new LogicRule(h, conclusions, certaintyFactor))
                .ToList();

            for (var i = 0; i < logicRules.Count;)
            {
                var tmpConclusions = new List<Fact>(logicRules[i].Conclusions);

                for (var j = 0; j < logicRules[i].Hypotheses.Count;)
                    if (tmpConclusions.Contains(logicRules[i].Hypotheses[j]))
                        tmpConclusions.Remove(logicRules[i].Hypotheses[j]);
                    else ++j;

                if (tmpConclusions.Count == 0)
                {
                    logicRules.RemoveAt(i);
                }
                else
                {
                    logicRules[i].Conclusions = tmpConclusions;

                    ++i;
                }
            }

            return logicRules;
        }

        [NotNull]
        private IReadOnlyList<IReadOnlyList<Fact>> ParseHypothese()
        {
            var hypothesisTable = new SymbolTable<Fact>();
            var hypothesisExpr = ParseExpr(hypothesisTable);

            var implicantId = 0;
            var implicants = new List<Implicant<int>>();

            var subsets = GetSubsets(hypothesisTable.Symbols);
            foreach (var subset in subsets)
            {
                var context = new RuntimeContext<Fact>(hypothesisTable);
                foreach (var hypothesis in subset)
                    context[hypothesis] = true;

                if (hypothesisExpr.Evaluate(context))
                {
                    var truthValues = hypothesisTable.Symbols
                        .Select(hypothesis => (bool?) context[hypothesis])
                        .ToList();

                    var implicant = new Implicant<int>(implicantId, truthValues);
                    implicants.Add(implicant);

                    implicantId++;
                }
            }

            var minimizedImplicants = QuineMcCluskey<int>.Minimize(implicants);

            var hypotheses = minimizedImplicants
                .Select(
                    minimizedImplicant => hypothesisTable.Symbols
                        .Where((symbol, i) => minimizedImplicant.Values[i] == true)
                        .ToList())
                .ToList();

            return hypotheses;
        }

        [NotNull]
        private IReadOnlyList<Fact> ParseConclusions()
        {
            var conclusionTable = new SymbolTable<Fact>();

            ParseFact(conclusionTable);

            while (CurrentToken.Type == And)
            {
                Eat(And);

                ParseFact(conclusionTable);
            }

            return conclusionTable.Symbols;
        }

        [NotNull]
        private Expr<Fact> ParseExpr([NotNull] SymbolTable<Fact> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var expr = ParseTerm(symbolTable);

            while (CurrentToken.Type == Or)
            {
                Eat(Or);

                var right = ParseTerm(symbolTable);

                expr = new OrExpr<Fact>(expr, right);
            }

            return expr;
        }

        [NotNull]
        private Expr<Fact> ParseTerm([NotNull] SymbolTable<Fact> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var expr = ParseFactor(symbolTable);

            while (CurrentToken.Type == And)
            {
                Eat(And);

                var right = ParseFactor(symbolTable);

                expr = new AndExpr<Fact>(expr, right);
            }

            return expr;
        }

        [NotNull]
        private Expr<Fact> ParseFactor([NotNull] SymbolTable<Fact> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            if (CurrentToken.Type == LParen)
            {
                Eat(LParen);

                var expr = ParseExpr(symbolTable);

                Eat(RParen);

                return expr;
            }

            return ParseFactExpr(symbolTable);
        }

        [NotNull]
        private IdentExpr<Fact> ParseFactExpr([NotNull] SymbolTable<Fact> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var fact = ParseFact(symbolTable);

            return new IdentExpr<Fact>(fact);
        }

        [NotNull]
        private Fact ParseFact([NotNull] SymbolTable<Fact> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var className = ParseIdentifier();

            Eat(Eq);

            var individualName = ParseIdentifier();

            Fact fact = new IndividualFact(className, individualName);

            symbolTable.Add(fact);

            return fact;
        }

        [NotNull]
        private IReadOnlyCollection<IReadOnlyCollection<Fact>> GetSubsets([NotNull] IReadOnlyList<Fact> set)
        {
            Check.NotEmpty(set, nameof(set));

            var subsets = new List<IReadOnlyCollection<Fact>>();

            var n = set.Count;
            for (var i = 1; i < 1 << n; i++)
            {
                var subset = new List<Fact>();

                for (var j = 0; j < n; j++)
                    if ((i & (1 << j)) > 0)
                        subset.Add(set[j]);

                subsets.Add(subset);
            }

            return subsets;
        }
    }
}