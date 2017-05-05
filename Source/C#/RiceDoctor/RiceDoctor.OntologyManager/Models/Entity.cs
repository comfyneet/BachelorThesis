using System;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.OntologyManager
{
    public abstract class Entity<T> : IEquatable<Entity<T>>
    {
        protected Entity([NotNull] string id, [CanBeNull] string label = null)
        {
            Check.NotEmpty(id, nameof(id));

            Id = id;
            Label = label;
        }

        public abstract EntityType Type { get; }

        [NotNull]
        public string Id { get; }

        [CanBeNull]
        public string Label { get; }

        public bool Equals(Entity<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id);
        }

        [NotNull]
        public static T Deserialize([NotNull] string json)
        {
            Check.NotEmpty(json, nameof(json));

            return JsonConvert.Deserialize<T>(json);
        }

        public override string ToString()
        {
            return Label ?? Id;
        }

        public static bool operator ==(Entity<T> entity1, Entity<T> entity2)
        {
            if (ReferenceEquals(entity1, entity2)) return true;
            if (ReferenceEquals(null, entity1)) return false;
            if (ReferenceEquals(null, entity2)) return false;
            return entity1.Equals(entity2);
        }

        public static bool operator !=(Entity<T> entity1, Entity<T> entity2)
        {
            return !(entity1 == entity2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Entity<T>) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}