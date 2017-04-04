using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.RuleManager.FuzzyTokenType;

namespace RiceDoctor.RuleManager
{
    public class FuzzyLexer : Lexer
    {
        [NotNull] private static readonly IReadOnlyDictionary<string, Token> ReservedKeywords;

        static FuzzyLexer()
        {
            ReservedKeywords = new Dictionary<string, Token>
            {
                {"FUNCTION", new Token(Function)},
                {"VARIABLE", new Token(Variable)},
                {"TERM", new Token(Term)},
                {"BEGIN", new Token(Begin)},
                {"END", new Token(End)},
                {"IF", new Token(If)},
                {"THEN", new Token(Then)},
                {"INPUT", new Token(Input)},
                {"OUTPUT", new Token(Output)}
            };
        }

        public FuzzyLexer([NotNull] string text) : base(text)
        {
        }

        protected override Token GetNextToken()
        {
            throw new NotImplementedException();
        }
    }
}