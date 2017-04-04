using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public abstract class Symbol
    {
        protected Symbol([NotNull] string ident, ExprType type)
        {
            Check.NotNull(ident, nameof(ident));

            Ident = ident;
            Type = type;
        }

        [NotNull]
        public string Ident { get; }

        public ExprType Type { get; }
    }
}