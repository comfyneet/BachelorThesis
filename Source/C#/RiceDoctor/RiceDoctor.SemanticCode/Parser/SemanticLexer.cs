using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.SemanticCode.SemanticTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.SemanticCode
{
    public class SemanticLexer : Lexer
    {
        public SemanticLexer([NotNull] string text) : base(text)
        {
        }

        protected override Token GetNextToken()
        {
            if (CurrentChar == '\r' || CurrentChar == '\n')
                return GetNewLine();

            if (CurrentChar == '[' && Peek() != '[')
            {
                Advance();
                return new Token(LSquare);
            }

            var lastToken = LastToken();
            if (lastToken != null)
            {
                var lastType = lastToken.Value.Type;

                if (lastType == LSquare)
                {
                    if (CurrentChar == '*')
                    {
                        Advance();
                        return new Token(Star);
                    }
                    if (CurrentChar == '/')
                    {
                        Advance();
                        return new Token(Slash);
                    }
                    if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                        return GetIdentifier();
                }
                if (lastType == Equal)
                {
                    if (CurrentChar == '"')
                        return GetUnquotedString();
                    if (CurrentChar == '-' || "0123456789".IndexOf(CurrentChar) != -1)
                        return GetNumber();
                    if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                        return GetIdentifier();
                }
                if (lastType == Slash)
                    if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                        return GetIdentifier();
                if (lastType == Ident)
                    if (CurrentChar == '=')
                    {
                        Advance();
                        return new Token(Equal);
                    }
                    else if (CurrentChar == ']')
                    {
                        Advance();
                        return new Token(RSquare);
                    }
                if (lastType == Number || lastType == UnquotedString ||
                    lastType == Star)
                    if (CurrentChar == ']')
                    {
                        Advance();
                        return new Token(RSquare);
                    }
            }

            if (CurrentChar == None)
                return new Token(Eof);

            return GetWords();
        }

        private Token GetWords()
        {
            var builder = new StringBuilder();
            while (CurrentChar != None)
            {
                if (CurrentChar == '[' && Peek() == '[')
                {
                    builder.Append(CurrentChar);
                    Advance(2);
                }
                else
                {
                    builder.Append(CurrentChar);
                    Advance();
                }

                if (CurrentChar == '[' && Peek() != '[' || CurrentChar == '\r' || CurrentChar == '\n')
                    break;
            }

            return new Token(Words, builder.ToString());
        }
    }
}