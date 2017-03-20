using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.OntologyManager
{
    public abstract class Entity<T>
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
    }
}