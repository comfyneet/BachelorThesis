using JetBrains.Annotations;

namespace RiceDoctor.FuzzyManager
{
    public abstract class Symbol
    {
        [NotNull]
        public abstract string Id { get; }

        public abstract override int GetHashCode();

        public abstract override bool Equals(object obj);

        public static bool operator ==(Symbol symbol1, Symbol symbol2)
        {
            if (ReferenceEquals(symbol1, symbol2)) return true;
            if (ReferenceEquals(null, symbol1)) return false;
            if (ReferenceEquals(null, symbol2)) return false;
            return symbol1.Equals(symbol2);
        }

        public static bool operator !=(Symbol symbol1, Symbol symbol2)
        {
            return !(symbol1 == symbol2);
        }
    }
}