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

            Class = className;
            Individual = individualName;
        }

        [NotNull]
        public string Class { get; }

        [NotNull]
        public string Individual { get; }

        public override string LValue => Class;

        public bool Equals(IndividualFact other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Class, other.Class) && string.Equals(Individual, other.Individual);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IndividualFact) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Class.GetHashCode() * 397) ^ Individual.GetHashCode();
            }
        }
    }
}