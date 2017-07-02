using System.Globalization;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class FuzzyFact : Fact
    {
        public FuzzyFact([NotNull] string variableName, double value)
        {
            Check.NotEmpty(variableName, nameof(variableName));

            Name = variableName;
            NumberValue = value;
            Value = NumberValue.ToString(CultureInfo.InvariantCulture);
        }

        public double NumberValue { get; }

        public override string Name { get; }

        public override string Value { get; }

        public bool Equals(FuzzyFact other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && string.Equals(Value, other.Value);
        }

        public static bool operator ==(FuzzyFact fuzzyFact1, FuzzyFact fuzzyFact2)
        {
            if (ReferenceEquals(fuzzyFact1, fuzzyFact2)) return true;
            if (ReferenceEquals(null, fuzzyFact1)) return false;
            if (ReferenceEquals(null, fuzzyFact2)) return false;
            return fuzzyFact1.Equals(fuzzyFact2);
        }

        public static bool operator !=(FuzzyFact fuzzyFact1, FuzzyFact fuzzyFact2)
        {
            return !(fuzzyFact1 == fuzzyFact2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FuzzyFact) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ Value.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }
}