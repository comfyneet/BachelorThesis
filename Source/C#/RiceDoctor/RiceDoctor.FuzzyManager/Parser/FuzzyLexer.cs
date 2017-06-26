using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.FuzzyManager.FuzzyTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.FuzzyManager
{
    public class FuzzyLexer : Lexer
    {
        [NotNull] private static readonly IReadOnlyDictionary<string, Token> ReservedKeywords;

        static FuzzyLexer()
        {
            ReservedKeywords = new Dictionary<string, Token>
            {
                {"function", new Token(Function)},
                {"variable", new Token(Variable)},
                {"term", new Token(Term)},
                {"begin", new Token(Begin)},
                {"end", new Token(End)},
                {"if", new Token(If)},
                {"then", new Token(Then)},
                {"INPUT", new Token(Input)},
                {"OUTPUT", new Token(Output)},
                {"and", new Token(And)},
                {"or", new Token(Or)},
                {"not", new Token(Not)}
            };
        }

        public FuzzyLexer([NotNull] string text) : base(text)
        {
        }

        protected override Token GetNextToken()
        {
            while (char.IsWhiteSpace(CurrentChar))
                Advance();

            if (CurrentChar == '+')
            {
                Advance();
                return new Token(Plus);
            }
            if (CurrentChar == '-')
            {
                Advance();
                return new Token(Minus);
            }
            if (CurrentChar == '*')
            {
                Advance();
                return new Token(Mul);
            }
            if (CurrentChar == '/')
            {
                Advance();
                return new Token(Div);
            }
            if (CurrentChar == '%')
            {
                Advance();
                return new Token(Mod);
            }
            if (CurrentChar == '(')
            {
                Advance();
                return new Token(LParen);
            }
            if (CurrentChar == ')')
            {
                Advance();
                return new Token(RParen);
            }
            if (CurrentChar == ':')
            {
                Advance();
                if (CurrentChar == '=')
                {
                    Advance();
                    return new Token(Assign);
                }
                return new Token(Colon);
            }
            if (CurrentChar == ';')
            {
                Advance();
                return new Token(Semi);
            }
            if (CurrentChar == ',')
            {
                Advance();
                return new Token(Comma);
            }
            if (CurrentChar == '=')
            {
                Advance();
                return new Token(Eq);
            }
            if (CurrentChar == '<')
            {
                Advance();
                if (CurrentChar == '>')
                {
                    Advance();
                    return new Token(Neq);
                }
                if (CurrentChar == '=')
                {
                    Advance();
                    return new Token(Lte);
                }
                return new Token(Lt);
            }
            if (CurrentChar == '>')
            {
                Advance();
                if (CurrentChar == '=')
                {
                    Advance();
                    return new Token(Gte);
                }
                return new Token(Gt);
            }
            if ("0123456789".IndexOf(CurrentChar) != -1)
                return GetNumber();
            if (char.IsLetter(CurrentChar) || CurrentChar == '_')
            {
                var id = GetIdentifier();
                return ReservedKeywords.TryGetValue((string) id.Value, out var keyword) ? keyword : id;
            }
            if (CurrentChar == None)
                return new Token(Eof);

            throw new InvalidOperationException($"The lexer cannot scan the current char '{CurrentChar}'.");
        }
    }
}