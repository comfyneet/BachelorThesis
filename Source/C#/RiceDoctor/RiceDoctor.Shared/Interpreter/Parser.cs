using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.Shared
{
    public abstract class Parser<T>
    {
        private int _pos;

        protected Parser([NotNull] Lexer lexer)
        {
            Check.NotNull(lexer, nameof(lexer));

            Tokens = lexer.Tokenize();
            _pos = 0;
            CurrentToken = Tokens[_pos];
        }

        [NotNull]
        protected IReadOnlyList<Token> Tokens { get; }

        protected Token CurrentToken { get; private set; }

        [CanBeNull]
        public abstract T Parse();

        protected void Eat([NotNull] TokenType type)
        {
            Check.NotNull(type, nameof(type));

            if (CurrentToken.Type == type)
            {
                _pos += 1;
                CurrentToken = _pos > Tokens.Count - 1 ? new Token(Illegal, Lexer.None) : Tokens[_pos];
            }
            else
            {
                throw new ArgumentException($"Token '{CurrentToken}' has a different type from '{type}'.");
            }
        }

        protected Token Peek(int length = 1)
        {
            var peekPos = _pos + length;

            if (peekPos >= Tokens.Count - 1) return new Token(Illegal, Lexer.None);
            return Tokens[peekPos];
        }

        [NotNull]
        protected string ParseIdentifier()
        {
            var ident = CurrentToken.Value;

            Eat(Ident);

            return (string) ident;
        }


        [NotNull]
        protected string ParseUnquotedString()
        {
            var unquotedString = CurrentToken.Value;

            Eat(UnquotedString);

            return (string) unquotedString;
        }

        protected double ParseNumber()
        {
            var number = CurrentToken.Value;

            Eat(Number);

            return (double) number;
        }
    }
}