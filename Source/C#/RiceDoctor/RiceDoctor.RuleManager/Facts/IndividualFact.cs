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
    }
}