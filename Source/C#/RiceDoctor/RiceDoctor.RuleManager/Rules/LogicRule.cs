using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class LogicRule : Rule, IEquatable<LogicRule>
    {
        public LogicRule([NotNull] IReadOnlyCollection<Fact> hypotheses, [NotNull] IReadOnlyCollection<Fact> conclusions,
            double certaintyFactor)
        {
            Check.NotNull(hypotheses, nameof(hypotheses));
            Check.NotNull(conclusions, nameof(conclusions));

            Hypotheses = hypotheses;
            Conclusions = conclusions;
            CertaintyFactor = certaintyFactor;
        }

        [NotNull]
        public IReadOnlyCollection<Fact> Hypotheses { get; }

        [NotNull]
        public IReadOnlyCollection<Fact> Conclusions { get; }

        public double CertaintyFactor { get; }

        public bool Equals(LogicRule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Hypotheses.Equals(other.Hypotheses) && Conclusions.Equals(other.Conclusions)
                   && Math.Abs(CertaintyFactor - other.CertaintyFactor) < double.Epsilon;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LogicRule);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Hypotheses.GetHashCode();
                hashCode = (hashCode * 397) ^ Conclusions.GetHashCode();
                hashCode = (hashCode * 397) ^ CertaintyFactor.GetHashCode();
                return hashCode;
            }
        }
    }
}