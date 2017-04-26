using System;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class ScalarFact : Fact, IEquatable<ScalarFact>
    {
        public ScalarFact([NotNull] string name, [NotNull] string value)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(value, nameof(value));

            Name = '"' + name + '"';
            Value = value;
        }

        public override string Name { get; }

        [NotNull]
        public string Value { get; }

        public bool Equals(ScalarFact other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ScalarFact) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ Value.GetHashCode();
            }
        }
    }
}