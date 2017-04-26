using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class LogicRule : IEquatable<LogicRule>
    {
        public LogicRule([NotNull] IReadOnlyCollection<Fact> hypotheses,
            [NotNull] IReadOnlyCollection<Fact> conclusions,
            double certaintyFactor)
        {
            Check.NotEmpty(hypotheses, nameof(hypotheses));
            Check.NotEmpty(conclusions, nameof(conclusions));

            Hypotheses = hypotheses;
            Conclusions = conclusions;
            CertaintyFactor = certaintyFactor;
        }

        [CanBeNull]
        public IReadOnlyCollection<Problem> Problems { get; internal set; }

        [NotNull]
        public IReadOnlyCollection<Fact> Hypotheses { get; }

        [NotNull]
        public IReadOnlyCollection<Fact> Conclusions { get; }

        public double CertaintyFactor { get; }

        public bool Equals(LogicRule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Hypotheses.ScrambledEqual(other.Hypotheses)
                   && Conclusions.ScrambledEqual(other.Conclusions)
                   && Math.Abs(CertaintyFactor - other.CertaintyFactor) < double.Epsilon;
        }

        public static bool operator ==(LogicRule rule1, LogicRule rule2)
        {
            if (ReferenceEquals(rule1, rule2)) return true;
            if (ReferenceEquals(null, rule1)) return false;
            if (ReferenceEquals(null, rule2)) return false;
            return rule1.Equals(rule2);
        }

        public static bool operator !=(LogicRule rule1, LogicRule rule2)
        {
            return !(rule1 == rule2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LogicRule) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Hypotheses.GetOrderIndependentHashCode();
                hashCode = (hashCode * 397) ^ Conclusions.GetOrderIndependentHashCode();
                hashCode = (hashCode * 397) ^ CertaintyFactor.GetHashCode();
                return hashCode;
            }
        }
    }
}