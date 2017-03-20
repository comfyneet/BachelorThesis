using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.SemanticCode
{
    public class SemanticTokenType : TokenType
    {
        static SemanticTokenType()
        {
            LSquare = new SemanticTokenType('[');
            RSquare = new SemanticTokenType(']');
            Slash = new SemanticTokenType('/');
            Equal = new SemanticTokenType('=');
            Star = new SemanticTokenType('*');
            Words = new SemanticTokenType("words");
        }

        protected SemanticTokenType([NotNull] string name) : base(name)
        {
        }

        protected SemanticTokenType(char name) : base(name)
        {
        }

        [NotNull]
        public static SemanticTokenType LSquare { get; }

        [NotNull]
        public static SemanticTokenType RSquare { get; }

        [NotNull]
        public static SemanticTokenType Slash { get; }

        [NotNull]
        public static SemanticTokenType Equal { get; }

        [NotNull]
        public static SemanticTokenType Star { get; }

        [NotNull]
        public static SemanticTokenType Words { get; }
    }
}