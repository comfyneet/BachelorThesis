using JetBrains.Annotations;

namespace RiceDoctor.RuleManager
{
    public abstract class Fact
    {
        [NotNull]
        public abstract string Name { get; }

        [NotNull]
        public abstract string Value { get; }

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();

        public abstract override string ToString();

        //public abstract string GetLabel();

        public static bool operator ==(Fact fact1, Fact fact2)
        {
            if (ReferenceEquals(fact1, fact2)) return true;
            if (ReferenceEquals(null, fact1)) return false;
            if (ReferenceEquals(null, fact2)) return false;
            return fact1.Equals(fact2);
        }

        public static bool operator !=(Fact fact1, Fact fact2)
        {
            return !(fact1 == fact2);
        }
    }
}