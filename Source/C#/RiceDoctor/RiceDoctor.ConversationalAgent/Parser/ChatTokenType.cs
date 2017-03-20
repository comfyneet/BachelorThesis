using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.ConversationalAgent
{
    public class ChatTokenType : TokenType
    {
        static ChatTokenType()
        {
            Plus = new ChatTokenType('+');
            Hyphen = new ChatTokenType('-');
            LParen = new ChatTokenType('(');
            RParen = new ChatTokenType(')');
            LSquare = new ChatTokenType('[');
            RSquare = new ChatTokenType(']');
            LBrace = new ChatTokenType('{');
            RBrace = new ChatTokenType('}');
            VertiBar = new ChatTokenType('|');
            Star = new ChatTokenType('*');
            Word = new ChatTokenType("word");
        }

        public ChatTokenType([NotNull] string name) : base(name)
        {
        }

        public ChatTokenType(char name) : base(name)
        {
        }

        [NotNull]
        public static ChatTokenType Plus { get; }

        [NotNull]
        public static ChatTokenType Hyphen { get; }

        [NotNull]
        public static ChatTokenType LParen { get; }

        [NotNull]
        public static ChatTokenType RParen { get; }

        [NotNull]
        public static ChatTokenType LSquare { get; }

        [NotNull]
        public static ChatTokenType RSquare { get; }

        [NotNull]
        public static ChatTokenType LBrace { get; }

        [NotNull]
        public static ChatTokenType RBrace { get; }

        [NotNull]
        public static ChatTokenType VertiBar { get; }

        [NotNull]
        public static ChatTokenType Star { get; }

        [NotNull]
        public static ChatTokenType Word { get; }
    }
}