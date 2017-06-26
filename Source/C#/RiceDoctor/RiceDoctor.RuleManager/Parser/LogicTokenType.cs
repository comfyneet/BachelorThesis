using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class LogicTokenType : TokenType
    {
        static LogicTokenType()
        {
            And = new LogicTokenType('&');
            Or = new LogicTokenType('|');
            Arrow = new LogicTokenType("->");
            LParen = new LogicTokenType('(');
            RParen = new LogicTokenType(')');
            LBrace = new LogicTokenType('{');
            RBrace = new LogicTokenType('}');
            Eq = new LogicTokenType('=');
            Semi = new LogicTokenType(';');
        }

        protected LogicTokenType([NotNull] string name) : base(name)
        {
        }

        protected LogicTokenType(char name) : base(name)
        {
        }

        [NotNull]
        public static LogicTokenType And { get; }

        [NotNull]
        public static LogicTokenType Or { get; }

        [NotNull]
        public static LogicTokenType Arrow { get; }

        [NotNull]
        public static LogicTokenType LParen { get; }

        [NotNull]
        public static LogicTokenType RParen { get; }

        [NotNull]
        public static LogicTokenType LBrace { get; }

        [NotNull]
        public static LogicTokenType RBrace { get; }

        [NotNull]
        public static LogicTokenType Eq { get; }

        [NotNull]
        public static LogicTokenType Semi { get; }
    }
}