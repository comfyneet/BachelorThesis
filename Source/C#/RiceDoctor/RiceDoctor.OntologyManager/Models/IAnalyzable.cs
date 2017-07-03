using JetBrains.Annotations;

namespace RiceDoctor.OntologyManager
{
    public interface IAnalyzable
    {
        [NotNull]
        string Id { get; }

        [CanBeNull]
        string Label { get; }
    }
}