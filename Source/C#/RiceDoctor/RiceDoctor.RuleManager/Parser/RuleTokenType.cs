using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class RuleTokenType : TokenType
    {
        static RuleTokenType()
        {
            And = new RuleTokenType('&');
            Or = new RuleTokenType('|');
            Arrow = new RuleTokenType("->");
            LParen = new RuleTokenType('(');
            RParen = new RuleTokenType(')');
            LBrace = new RuleTokenType('{');
            RBrace = new RuleTokenType('}');
            Eq = new RuleTokenType('=');
            Semi = new RuleTokenType(';');
        }

        protected RuleTokenType([NotNull] string name) : base(name)
        {
        }

        protected RuleTokenType(char name) : base(name)
        {
        }

        [NotNull]
        public static RuleTokenType And { get; }

        [NotNull]
        public static RuleTokenType Or { get; }

        [NotNull]
        public static RuleTokenType Arrow { get; }

        [NotNull]
        public static RuleTokenType LParen { get; }

        [NotNull]
        public static RuleTokenType RParen { get; }

        [NotNull]
        public static RuleTokenType LBrace { get; }

        [NotNull]
        public static RuleTokenType RBrace { get; }

        [NotNull]
        public static RuleTokenType Eq { get; }

        [NotNull]
        public static RuleTokenType Semi { get; }
    }
}