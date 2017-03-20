using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class TokenType : EnumType
    {
        static TokenType()
        {
            Illegal = new TokenType("Illegal");
            Eof = new TokenType("end of file");
            NewLine = new TokenType("new line");
            Ident = new TokenType("identifier");
            Number = new TokenType("number");
            UnquotedString = new TokenType("unquoted string");
        }

        protected TokenType([NotNull] string name) : base(name)
        {
        }

        protected TokenType(char name) : base(name.ToString())
        {
        }

        [NotNull]
        public static TokenType Illegal { get; }

        [NotNull]
        public static TokenType Eof { get; }

        [NotNull]
        public static TokenType NewLine { get; }

        [NotNull]
        public static TokenType Ident { get; }

        [NotNull]
        public static TokenType Number { get; }

        [NotNull]
        public static TokenType UnquotedString { get; }
    }
}