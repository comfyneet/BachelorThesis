using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class Implicant<T> : IEquatable<Implicant<T>>
    {
        public Implicant([NotNull] T identifier, [NotNull] IReadOnlyList<bool?> values)
        {
            Check.NotNull(identifier, nameof(identifier));
            Check.NotEmpty(values, nameof(values));

            Identifiers = new List<T> {identifier};
            Values = values;
        }

        public Implicant([NotNull] IReadOnlyList<T> identifiers, [NotNull] IReadOnlyList<bool?> values)
        {
            Check.NotEmpty(identifiers, nameof(identifiers));
            Check.NotEmpty(values, nameof(values));

            Identifiers = identifiers;
            Values = values;
        }

        public int Size => Identifiers.Count;

        [NotNull]
        public IReadOnlyList<T> Identifiers { get; }

        [NotNull]
        public IReadOnlyList<bool?> Values { get; }

        public bool Equals(Implicant<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifiers.ScrambledEqual(other.Identifiers) && Values.SequenceEqual(other.Values);
        }

        public static bool operator ==(Implicant<T> implicant1, Implicant<T> implicant2)
        {
            if (ReferenceEquals(implicant1, implicant2)) return true;
            if (ReferenceEquals(null, implicant1)) return false;
            if (ReferenceEquals(null, implicant2)) return false;
            return implicant1.Equals(implicant2);
        }

        public static bool operator !=(Implicant<T> implicant1, Implicant<T> implicant2)
        {
            return !(implicant1 == implicant2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Implicant<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Identifiers.GetOrderIndependentHashCode();
                foreach (var value in Values)
                    hashCode = (hashCode * 397) ^ value.GetHashCode();

                return hashCode;
            }
        }
    }
}