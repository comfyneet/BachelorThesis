using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class FuzzyTokenType : TokenType
    {
        static FuzzyTokenType()
        {
            Function = new FuzzyTokenType("function");
            Variable = new FuzzyTokenType("variable");
            Term = new FuzzyTokenType("term");
            Begin = new FuzzyTokenType("begin");
            End = new FuzzyTokenType("end");
            If = new FuzzyTokenType("if");
            Then = new FuzzyTokenType("then");
            Input = new FuzzyTokenType("INPUT");
            Output = new FuzzyTokenType("OUTPUT");
            Plus = new FuzzyTokenType('+');
            Minus = new FuzzyTokenType('-');
            Mul = new FuzzyTokenType('*');
            Div = new FuzzyTokenType('/');
            Mod = new FuzzyTokenType('%');
            LParen = new FuzzyTokenType('(');
            RParen = new FuzzyTokenType(')');
            Assign = new FuzzyTokenType(":=");
            Semi = new FuzzyTokenType(';');
            Colon = new FuzzyTokenType(':');
            Comma = new FuzzyTokenType(',');
            And = new FuzzyTokenType("and");
            Or = new FuzzyTokenType("or");
            Not = new FuzzyTokenType("not");
            Eq = new FuzzyTokenType('=');
            Neq = new FuzzyTokenType("<>");
            Gt = new FuzzyTokenType('>');
            Gte = new FuzzyTokenType(">=");
            Lt = new FuzzyTokenType('<');
            Lte = new FuzzyTokenType("<=");
        }

        protected FuzzyTokenType([NotNull] string name) : base(name)
        {
        }

        protected FuzzyTokenType(char name) : base(name)
        {
        }

        [NotNull]
        public static FuzzyTokenType Function { get; }

        [NotNull]
        public static FuzzyTokenType Variable { get; }

        [NotNull]
        public static FuzzyTokenType Term { get; }

        [NotNull]
        public static FuzzyTokenType Begin { get; }

        [NotNull]
        public static FuzzyTokenType End { get; }

        [NotNull]
        public static FuzzyTokenType If { get; }

        [NotNull]
        public static FuzzyTokenType Then { get; }

        [NotNull]
        public static FuzzyTokenType Input { get; }

        [NotNull]
        public static FuzzyTokenType Output { get; }

        [NotNull]
        public static FuzzyTokenType Plus { get; }

        [NotNull]
        public static FuzzyTokenType Minus { get; }

        [NotNull]
        public static FuzzyTokenType Mul { get; }

        [NotNull]
        public static FuzzyTokenType Div { get; }

        [NotNull]
        public static FuzzyTokenType Mod { get; }

        [NotNull]
        public static FuzzyTokenType LParen { get; }

        [NotNull]
        public static FuzzyTokenType RParen { get; }

        [NotNull]
        public static FuzzyTokenType Assign { get; }

        [NotNull]
        public static FuzzyTokenType Semi { get; }

        [NotNull]
        public static FuzzyTokenType Colon { get; }

        [NotNull]
        public static FuzzyTokenType Comma { get; }

        [NotNull]
        public static FuzzyTokenType And { get; }

        [NotNull]
        public static FuzzyTokenType Or { get; }

        [NotNull]
        public static FuzzyTokenType Not { get; }

        [NotNull]
        public static FuzzyTokenType Eq { get; }

        [NotNull]
        public static FuzzyTokenType Neq { get; }

        [NotNull]
        public static FuzzyTokenType Gt { get; }

        [NotNull]
        public static FuzzyTokenType Gte { get; }

        [NotNull]
        public static FuzzyTokenType Lt { get; }

        [NotNull]
        public static FuzzyTokenType Lte { get; }
    }
}