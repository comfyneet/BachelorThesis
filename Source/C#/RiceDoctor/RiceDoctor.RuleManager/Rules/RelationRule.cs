using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class RelationRule : Rule, IEquatable<RelationRule>
    {
        public RelationRule(
            [NotNull] string domain,
            [NotNull] string range,
            [NotNull] IReadOnlyCollection<string> inferredDomains,
            [NotNull] IReadOnlyCollection<string> inferredRanges,
            [NotNull] IReadOnlyCollection<string> relations)
        {
            Check.NotEmpty(domain, nameof(domain));
            Check.NotEmpty(range, nameof(range));
            Check.NotEmpty(inferredDomains, nameof(inferredDomains));
            Check.NotEmpty(inferredRanges, nameof(inferredRanges));
            Check.NotEmpty(relations, nameof(relations));

            Domain = domain;
            Range = range;
            InferredDomains = inferredDomains;
            InferredRanges = inferredRanges;
            Relations = relations;
        }

        [NotNull]
        public string Domain { get; }

        [NotNull]
        public string Range { get; }

        [NotNull]
        public IReadOnlyCollection<string> InferredDomains { get; }

        [NotNull]
        public IReadOnlyCollection<string> InferredRanges { get; }

        [NotNull]
        public IReadOnlyCollection<string> Relations { get; }

        public bool Equals(RelationRule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Domain, other.Domain) && string.Equals(Range, other.Range);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((RelationRule) obj);
        }

        public static bool operator ==(RelationRule relationRule1, RelationRule relationRule2)
        {
            if (ReferenceEquals(relationRule1, relationRule2)) return true;
            if (ReferenceEquals(null, relationRule1)) return false;
            if (ReferenceEquals(null, relationRule2)) return false;
            return relationRule1.Equals(relationRule2);
        }

        public static bool operator !=(RelationRule relationRule1, RelationRule relationRule2)
        {
            return !(relationRule1 == relationRule2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Domain.GetHashCode() * 397) ^ Range.GetHashCode();
            }
        }
    }
}