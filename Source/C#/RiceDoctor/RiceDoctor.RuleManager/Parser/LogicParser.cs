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

            var certaintyFactor = 1.0;
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
            var hypothesisTable = new SymbolTable();
            var hypothesisExpr = ParseExpr(hypothesisTable);

            var hypotheses = new List<IReadOnlyCollection<Fact>>();
            var hypothesisSubsets = GetSubsets(hypothesisTable.Symbols.ToList());
            foreach (var hypothesisSubset in hypothesisSubsets)
            {
                var context = new RuntimeContext(hypothesisTable);
                foreach (var hypothesis in hypothesisSubset)
                    context[hypothesis] = true;

                if (hypothesisExpr.Evaluate(context))
                    hypotheses.Add(hypothesisSubset);
            }

            return hypotheses.AsReadOnly();
        }

        [NotNull]
        private IReadOnlyCollection<Fact> ParseConclusions()
        {
            var conclusionTable = new SymbolTable();

            ParseFact(conclusionTable);

            while (CurrentToken.Type == And)
            {
                Eat(And);

                ParseFact(conclusionTable);
            }

            return conclusionTable.Symbols;
        }

        [NotNull]
        private Expr ParseExpr([NotNull] SymbolTable symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var expr = ParseTerm(symbolTable);

            while (CurrentToken.Type == Or)
            {
                Eat(Or);

                var right = ParseTerm(symbolTable);

                expr = new OrExpr(expr, right);
            }

            return expr;
        }

        [NotNull]
        private Expr ParseTerm([NotNull] SymbolTable symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var expr = ParseFactor(symbolTable);

            while (CurrentToken.Type == And)
            {
                Eat(And);

                var right = ParseFactor(symbolTable);

                expr = new AndExpr(expr, right);
            }

            return expr;
        }

        [NotNull]
        private Expr ParseFactor([NotNull] SymbolTable symbolTable)
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
        private FactExpr ParseFactExpr([NotNull] SymbolTable symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var fact = ParseFact(symbolTable);

            return new FactExpr(fact);
        }

        [NotNull]
        private Fact ParseFact([NotNull] SymbolTable symbolTable)
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
            Check.NotNull(set, nameof(set));

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