using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public struct Token
    {
        public Token([NotNull] TokenType type)
        {
            Check.NotNull(type, nameof(type));
            Check.NotEmpty(type.Name, nameof(type.Name));

            Type = type;
            Value = type.Name;
        }

        public Token([NotNull] TokenType type, [NotNull] object value)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(value, nameof(value));

            Type = type;
            Value = value;
        }

        [NotNull]
        public TokenType Type { get; }

        [NotNull]
        public object Value { get; }
    }
}