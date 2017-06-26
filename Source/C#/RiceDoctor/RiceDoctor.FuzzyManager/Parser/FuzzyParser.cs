using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.FuzzyManager.BinaryOp;
using static RiceDoctor.FuzzyManager.FuzzyTokenType;
using static RiceDoctor.FuzzyManager.UnaryOp;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.FuzzyManager
{
    public class FuzzyParser : Parser<Tuple<IReadOnlyCollection<FunctionSymbol>, IReadOnlyCollection<VariableSymbol>>>
    {
        [NotNull] private readonly IDictionary<TokenType, BinaryOp> _addingOps = new Dictionary<TokenType, BinaryOp>
        {
            {Plus, Add},
            {Minus, Subtract},
            {FuzzyTokenType.Or, BinaryOp.Or}
        };

        [NotNull] private readonly IDictionary<TokenType, BinaryOp> _multiplyingOps =
            new Dictionary<TokenType, BinaryOp>
            {
                {Mul, Multiply},
                {Div, Divide},
                {Mod, Modulo},
                {FuzzyTokenType.And, BinaryOp.And}
            };

        [NotNull] private readonly IDictionary<TokenType, BinaryOp> _relationalOps = new Dictionary<TokenType, BinaryOp>
        {
            {Eq, Equal},
            {Neq, NotEqual},
            {Lt, Less},
            {Lte, LessEqual},
            {Gt, Greater},
            {Gte, GreaterEqual}
        };

        [NotNull] private readonly IDictionary<TokenType, UnaryOp> _signs = new Dictionary<TokenType, UnaryOp>
        {
            {Plus, Positive},
            {Minus, Negative}
        };

        [CanBeNull] private Tuple<IReadOnlyCollection<FunctionSymbol>, IReadOnlyCollection<VariableSymbol>> _symbols;

        public FuzzyParser([NotNull] FuzzyLexer lexer) : base(lexer)
        {
        }

        [NotNull]
        public override Tuple<IReadOnlyCollection<FunctionSymbol>, IReadOnlyCollection<VariableSymbol>> Parse()
        {
            if (_symbols != null) return _symbols;

            var functionSymbols = ParseFunctions();
            var variableSymbols = ParseVariables(functionSymbols);
            _symbols =
                new Tuple<IReadOnlyCollection<FunctionSymbol>, IReadOnlyCollection<VariableSymbol>>(
                    functionSymbols,
                    variableSymbols);

            if (CurrentToken.Type != Eof)
                throw new InvalidOperationException(CoreStrings.SyntaxError(Eof.Name, CurrentToken.Type.Name));

            return _symbols;
        }

        [NotNull]
        private IReadOnlyCollection<FunctionSymbol> ParseFunctions()
        {
            var functionSymbols = new List<FunctionSymbol>();

            while (CurrentToken.Type == Function)
            {
                Eat(Function);

                var id = ParseIdentifier();

                var @params = ParseParams();

                var funcStmt = ParseFunctionStmt(@params);

                Eat(Semi);

                var functionSymbol = new FunctionSymbol(id, @params, funcStmt);
                if (functionSymbols.Contains(functionSymbol)) throw new Exception(CoreStrings.CannotDeclareAgain(id));

                functionSymbols.Add(functionSymbol);
            }

            return functionSymbols;
        }

        [NotNull]
        private IReadOnlyList<string> ParseParams()
        {
            Eat(LParen);

            var @params = new List<string>();
            while (CurrentToken.Type != RParen)
            {
                var param = ParseIdentifier();
                if (@params.Contains(param)) throw new Exception(CoreStrings.CannotDeclareAgain(param));

                @params.Add(param);

                if (CurrentToken.Type != RParen) Eat(Comma);
            }

            Eat(RParen);

            return @params;
        }

        [NotNull]
        private FunctionStmt ParseFunctionStmt([NotNull] IReadOnlyCollection<string> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            Eat(Begin);

            var ifStmts = new List<IfStmt>();

            while (CurrentToken.Type == If)
            {
                Eat(If);

                var condition = ParseExpr(symbolTable);

                Eat(Then);

                var outputStmt = ParseOutputStmt(symbolTable);

                ifStmts.Add(new IfStmt(condition, outputStmt));

                Eat(Semi);
            }

            Eat(End);

            return new FunctionStmt(ifStmts);
        }

        [NotNull]
        private Expr ParseExpr([NotNull] IReadOnlyCollection<string> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var expr = ParseSimpleExpr(symbolTable);

            if (_relationalOps.TryGetValue(CurrentToken.Type, out var relaOp))
            {
                Eat(CurrentToken.Type);

                var rightExpr = ParseSimpleExpr(symbolTable);
                return new BinaryExpr(expr, relaOp, rightExpr);
            }

            return expr;
        }

        [NotNull]
        private OutputStmt ParseOutputStmt([NotNull] IReadOnlyCollection<string> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            Eat(Output);

            Eat(Assign);

            var expr = ParseExpr(symbolTable);

            return new OutputStmt(expr);
        }

        [NotNull]
        private Expr ParseSimpleExpr([NotNull] IReadOnlyCollection<string> symbolTable)
        {
            Expr ParseNode(Expr node)
            {
                if (_addingOps.TryGetValue(CurrentToken.Type, out var addingOp))
                {
                    Eat(CurrentToken.Type);

                    var rightExpr = ParseTerm(symbolTable);
                    return ParseNode(new BinaryExpr(node, addingOp, rightExpr));
                }

                return node;
            }

            Check.NotNull(symbolTable, nameof(symbolTable));

            Expr expr;
            if (_signs.TryGetValue(CurrentToken.Type, out var op))
            {
                Eat(CurrentToken.Type);

                expr = new UnaryExpr(op, ParseTerm(symbolTable));
            }
            else
            {
                expr = ParseTerm(symbolTable);
            }

            return ParseNode(expr);
        }

        [NotNull]
        private Expr ParseTerm([NotNull] IReadOnlyCollection<string> symbolTable)
        {
            Expr ParseNode(Expr node)
            {
                if (_multiplyingOps.TryGetValue(CurrentToken.Type, out var mulOp))
                {
                    Eat(CurrentToken.Type);

                    var rightExpr = ParseFactor(symbolTable);
                    return ParseNode(new BinaryExpr(node, mulOp, rightExpr));
                }

                return node;
            }

            Check.NotNull(symbolTable, nameof(symbolTable));

            var expr = ParseFactor(symbolTable);

            return ParseNode(expr);
        }

        [NotNull]
        private Expr ParseFactor([NotNull] IReadOnlyCollection<string> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            if (CurrentToken.Type == Number)
            {
                var number = ParseNumber();

                return new NumberExpr(new NumberValue(number));
            }
            if (CurrentToken.Type == LParen)
            {
                Eat(LParen);

                var expr = ParseExpr(symbolTable);

                Eat(RParen);

                return expr;
            }
            if (CurrentToken.Type == FuzzyTokenType.Not)
            {
                Eat(FuzzyTokenType.Not);

                var expr = ParseExpr(symbolTable);

                return new UnaryExpr(UnaryOp.Not, expr);
            }
            if (CurrentToken.Type == Input)
            {
                Eat(Input);

                return new ParamExpr("INPUT");
            }

            var id = ParseIdentifier();
            if (!symbolTable.Contains(id))
                throw new Exception(CoreStrings.NonexistentDeclare(id));

            return new ParamExpr(id);
        }

        [NotNull]
        private IReadOnlyCollection<VariableSymbol> ParseVariables(
            [NotNull] IReadOnlyCollection<FunctionSymbol> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            var variableSymbols = new List<VariableSymbol>();

            while (CurrentToken.Type == Variable)
            {
                Eat(Variable);

                var id = ParseIdentifier();

                var variableStmt = ParseVariableStmt(symbolTable);

                Eat(Semi);

                var variableSymbol = new VariableSymbol(id, variableStmt);
                if (variableSymbols.Contains(variableSymbol)) throw new Exception(CoreStrings.CannotDeclareAgain(id));

                variableSymbols.Add(variableSymbol);
            }

            return variableSymbols;
        }

        [NotNull]
        private VariableStmt ParseVariableStmt([NotNull] IReadOnlyCollection<FunctionSymbol> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            Eat(Begin);

            var terms = new Dictionary<string, Tuple<FunctionSymbol, IReadOnlyList<NumberValue>>>();

            while (CurrentToken.Type == Term)
            {
                Eat(Term);

                var id = ParseIdentifier();
                if (terms.ContainsKey(id)) throw new Exception(CoreStrings.CannotDeclareAgain(id));

                Eat(Colon);

                var functionId = ParseIdentifier();
                var functionSymbol = symbolTable.FirstOrDefault(f => f.Id == functionId);
                if (functionSymbol == null) throw new Exception(CoreStrings.NonexistentDeclare(functionId));

                Eat(LParen);

                var args = new List<NumberValue>();
                while (CurrentToken.Type != RParen)
                {
                    double value;
                    if (CurrentToken.Type == Minus)
                    {
                        Eat(Minus);
                        value = -ParseNumber();
                    }
                    else
                    {
                        value = ParseNumber();
                    }

                    args.Add(new NumberValue(value));

                    if (CurrentToken.Type != RParen) Eat(Comma);
                }

                Eat(RParen);

                Eat(Semi);

                if (args.Count != functionSymbol.Params.Count)
                    throw new Exception(
                        $"Argument element count is different than function '{functionId}' parameter element count.");

                terms.Add(id, new Tuple<FunctionSymbol, IReadOnlyList<NumberValue>>(functionSymbol, args));
            }

            Eat(End);

            return new VariableStmt(terms);
        }
    }
}