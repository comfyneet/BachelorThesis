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
            return _guid == other?._guid && Name == other.Name;
        }
    }
}