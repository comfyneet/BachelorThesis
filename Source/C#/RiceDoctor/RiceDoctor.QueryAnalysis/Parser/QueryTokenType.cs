﻿using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryAnalysis
{
    public class QueryTokenType : TokenType
    {
        static QueryTokenType()
        {
            Colon = new QueryTokenType(':');
            LParen = new QueryTokenType('(');
            RParen = new QueryTokenType(')');
            LSquare = new QueryTokenType('[');
            RSquare = new QueryTokenType(']');
            LBrace = new QueryTokenType('{');
            RBrace = new QueryTokenType('}');
            VertiBar = new QueryTokenType('|');
            Star = new QueryTokenType('*');
            Comma = new QueryTokenType(',');
            Word = new QueryTokenType("word");
        }

        public QueryTokenType([NotNull] string name) : base(name)
        {
        }

        public QueryTokenType(char name) : base(name)
        {
        }

        [NotNull]
        public static QueryTokenType Colon { get; }

        [NotNull]
        public static QueryTokenType LParen { get; }

        [NotNull]
        public static QueryTokenType RParen { get; }

        [NotNull]
        public static QueryTokenType LSquare { get; }

        [NotNull]
        public static QueryTokenType RSquare { get; }

        [NotNull]
        public static QueryTokenType LBrace { get; }

        [NotNull]
        public static QueryTokenType RBrace { get; }

        [NotNull]
        public static QueryTokenType VertiBar { get; }

        [NotNull]
        public static QueryTokenType Star { get; }

        [NotNull]
        public static QueryTokenType Comma { get; }

        [NotNull]
        public static QueryTokenType Word { get; }
    }
}