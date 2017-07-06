using System;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public abstract class EnumType : IEquatable<EnumType>
    {
        private readonly Guid _guid;

        protected EnumType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
            _guid = Guid.NewGuid();
        }

        [NotNull]
        public string Name { get; }

        public bool Equals(EnumType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _guid == other._guid && string.Equals(Name, other.Name);
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_guid.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EnumType);
        }

        public static bool operator ==(EnumType type1, EnumType type2)
        {
            if (ReferenceEquals(type1, type2)) return true;
            if (ReferenceEquals(null, type1)) return false;
            if (ReferenceEquals(null, type2)) return false;
            return type1.Equals(type2);
        }

        public static bool operator !=(EnumType type1, EnumType type2)
        {
            return !(type1 == type2);
        }
    }
}