using System;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.RuleManager.LogicTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.RuleManager
{
    public class LogicLexer : Lexer
    {
        public LogicLexer([NotNull] string text) : base(text)
        {
        }

        protected override Token GetNextToken()
        {
            while (char.IsWhiteSpace(CurrentChar) && CurrentChar != '\r' && CurrentChar != '\n')
                Advance();

            if (CurrentChar == '\r' || CurrentChar == '\n')
                return GetNewLine();

            if (CurrentChar == '&')
            {
                Advance();
                return new Token(And);
            }
            if (CurrentChar == '|')
            {
                Advance();
                return new Token(Or);
            }
            if (CurrentChar == '-' && Peek() == '>')
            {
                Advance(2);
                return new Token(Arrow);
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
            if (CurrentChar == '{')
            {
                Advance();
                return new Token(LBrace);
            }
            if (CurrentChar == '}')
            {
                Advance();
                return new Token(RBrace);
            }
            if (CurrentChar == '=')
            {
                Advance();
                return new Token(Eq);
            }
            if (CurrentChar == '"')
            {
                var unquotedString = GetUnquotedString();

                // Simple checks if knowledge experts forget to insert '"' at the end of scalar facts
                if (((string) unquotedString.Value).IndexOfAny(new[] {'"', '=', '\n'}) != -1)
                    throw new InvalidOperationException(
                        $"The lexer cannot scan the scalar fact \"{unquotedString.Value}\" correctly.");

                return unquotedString;
            }
            if (CurrentChar == '-' || "0123456789".IndexOf(CurrentChar) != -1)
                return GetNumber();
            if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                return GetIdentifier();
            if (CurrentChar == None)
                return new Token(Eof);

            throw new InvalidOperationException($"The lexer cannot scan the current char '{CurrentChar}'.");
        }
    }
}