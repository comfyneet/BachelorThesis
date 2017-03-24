using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.RuleManager.LogicTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.RuleManager
{
    public class LogicParser : Parser<IReadOnlyCollection<LogicRule>>
    {
        [CanBeNull] private ReadOnlyCollection<LogicRule> _logicRules;

        public LogicParser([NotNull] LogicLexer lexer) : base(lexer)
        {
        }

        [NotNull]
        public override IReadOnlyCollection<LogicRule> Parse()
        {
            if (_logicRules != null) return _logicRules;

            _logicRules = ParseLogicRuleList();

            if (CurrentToken.Type != Eof)
                throw new InvalidOperationException(CoreStrings.SyntaxError(Eof.Name, CurrentToken.Type.Name));

            return _logicRules;
        }

        [NotNull]
        private ReadOnlyCollection<LogicRule> ParseLogicRuleList()
        {
            var logicRuleList = new List<LogicRule>();

            var logicRules = ParseLogicRules();
            logicRuleList.AddRange(logicRules);

            while (CurrentToken.Type != Eof)
            {
                Eat(NewLine);
                while (CurrentToken.Type == NewLine)
                    Eat(NewLine);

                logicRules = ParseLogicRules();
                logicRuleList.AddRange(logicRules);
            }

            return logicRuleList.AsReadOnly();
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

            var logicRules = hypotheses
                .Select(h => new LogicRule(h, conclusions, certaintyFactor))
                .ToList()
                .AsReadOnly();

            return logicRules;
        }

        [NotNull]
        private IReadOnlyCollection<IReadOnlyCollection<Fact>> ParseHypothese()
        {
            var hypothesisTable = new SymbolTable<Fact>();
            var hypothesisExpr = ParseExpr(hypothesisTable);

            var implicantId = 0;
            var implicants = new List<Implicant<int>>();

            var subsets = GetSubsets(hypothesisTable.Symbols.ToList());
            foreach (var subset in subsets)
            {
                var context = new RuntimeContext<Fact>(hypothesisTable);
                foreach (var hypothesis in subset)
                    context[hypothesis] = true;

                if (hypothesisExpr.Evaluate(context))
                {
                    var truthValues = hypothesisTable.Symbols
                        .Select(hypothesis => (bool?) context[hypothesis])
                        .ToList()
                        .AsReadOnly();

                    var implicant = new Implicant<int>(implicantId, truthValues);
                    implicants.Add(implicant);

                    implicantId++;
                }
            }

            var minimizedImplicants = QuineMcCluskey<int>.Minimize(implicants.AsReadOnly());

            var hypotheses = minimizedImplicants
                .Select(
                    minimizedImplicant => hypothesisTable.Symbols
                        .Where((symbol, i) => minimizedImplicant.Values[i] == true)
                        .ToList()
                        .AsReadOnly())
                .ToList()
                .AsReadOnly();

            return hypotheses;
        }

        [NotNull]
        private IReadOnlyCollection<Fact> ParseConclusions()
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

            Fact fact;
            if (CurrentToken.Type == Ident)
            {
                var className = ParseIdentifier();

                Eat(Equal);

                var individualName = ParseIdentifier();

                fact = new IndividualFact(className, individualName);
            }
            else
            {
                var name = ParseUnquotedString();

                Eat(Equal);

                var value = ParseUnquotedString();

                fact = new ScalarFact(name, value);
            }

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

                subsets.Add(subset.AsReadOnly());
            }

            return subsets.AsReadOnly();
        }
    }
}