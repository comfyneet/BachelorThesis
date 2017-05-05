using System;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class IndividualFact : Fact, IEquatable<IndividualFact>
    {
        public IndividualFact([NotNull] string className, [NotNull] string individualName)
        {
            Check.NotEmpty(className, nameof(className));
            Check.NotEmpty(individualName, nameof(individualName));

            Name = className;
            Individual = individualName;
        }

        public override string Name { get; }

        [NotNull]
        public string Individual { get; }

        public bool Equals(IndividualFact other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && string.Equals(Individual, other.Individual);
        }

        public static bool operator ==(IndividualFact individualFact1, IndividualFact individualFact2)
        {
            if (ReferenceEquals(individualFact1, individualFact2)) return true;
            if (ReferenceEquals(null, individualFact1)) return false;
            if (ReferenceEquals(null, individualFact2)) return false;
            return individualFact1.Equals(individualFact2);
        }

        public static bool operator !=(IndividualFact individualFact1, IndividualFact individualFact2)
        {
            return !(individualFact1 == individualFact2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IndividualFact) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode() * 397) ^ Individual.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{Name}={Individual}";
        }
    }
}