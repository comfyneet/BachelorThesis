using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class LogicRule : Rule, IEquatable<LogicRule>
    {
        public LogicRule(
            [NotNull] IReadOnlyList<Fact> hypotheses,
            [NotNull] IReadOnlyList<Fact> conclusions,
            double certaintyFactor)
        {
            Check.NotEmpty(hypotheses, nameof(hypotheses));
            Check.NotEmpty(conclusions, nameof(conclusions));

            Hypotheses = hypotheses;
            Conclusions = conclusions;
            CertaintyFactor = certaintyFactor;
        }

        [NotNull]
        public IReadOnlyList<Fact> Hypotheses { get; internal set; }

        [NotNull]
        public IReadOnlyList<Fact> Conclusions { get; internal set; }

        public double CertaintyFactor { get; }

        public bool Equals(LogicRule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Hypotheses.ScrambledEqual(other.Hypotheses)
                   && Conclusions.ScrambledEqual(other.Conclusions)
                   && CertaintyFactor.Equals3DigitPrecision(other.CertaintyFactor);
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
                return hashCode;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            for (var i = 0; i < Hypotheses.Count; ++i)
            {
                if (i > 0) builder.Append(" & ");
                builder.Append(Hypotheses[i]);
            }

            builder.Append(" -> ");

            for (var i = 0; i < Conclusions.Count; ++i)
            {
                if (i > 0) builder.Append(" & ");
                builder.Append(Conclusions[i]);
            }

            builder.Append($" {{{CertaintyFactor}}}");

            return builder.ToString();
        }
    }
}