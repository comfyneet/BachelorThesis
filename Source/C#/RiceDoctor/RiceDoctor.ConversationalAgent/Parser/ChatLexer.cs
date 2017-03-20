using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using static RiceDoctor.ConversationalAgent.ChatTokenType;
using static RiceDoctor.Shared.TokenType;

namespace RiceDoctor.ConversationalAgent
{
    public class ChatLexer : Lexer
    {
        public ChatLexer([NotNull] string text) : base(text)
        {
        }

        protected override Token GetNextToken()
        {
            if (CurrentChar == '\r' || CurrentChar == '\n')
                return GetNewLine();

            while (char.IsWhiteSpace(CurrentChar) && CurrentChar != '\r' && CurrentChar != '\n')
                SkipWhitespace();

            if (CurrentChar == '+')
            {
                Advance();
                return new Token(Plus);
            }
            if (CurrentChar == '-')
            {
                Advance();
                return new Token(Hyphen);
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
            if (CurrentChar == '[')
            {
                Advance();
                return new Token(LSquare);
            }
            if (CurrentChar == ']')
            {
                Advance();
                return new Token(RSquare);
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
            if (CurrentChar == '|')
            {
                Advance();
                return new Token(VertiBar);
            }
            if (CurrentChar == '*')
            {
                Advance();
                return new Token(Star);
            }

            while (!char.IsLetterOrDigit(CurrentChar) && CurrentChar != None)
                Advance();

            if (CurrentChar == None)
                return new Token(Eof);

            return GetWord();
        }

        private Token GetWord()
        {
            var builder = new StringBuilder();
            while (char.IsLetterOrDigit(CurrentChar))
            {
                builder.Append(char.ToLower(CurrentChar));
                Advance();
            }

            return new Token(Word, builder.ToString());
        }
    }
}