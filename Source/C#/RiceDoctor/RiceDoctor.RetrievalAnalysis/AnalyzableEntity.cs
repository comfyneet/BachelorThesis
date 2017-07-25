using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;

namespace RiceDoctor.RetrievalAnalysis
{
    public class AnalyzableEntity
    {
        public AnalyzableEntity(
            [NotNull] IAnalyzable entity,
            [CanBeNull] IReadOnlyCollection<string> synonyms,
            [CanBeNull] IReadOnlyCollection<string> nearSynonyms,
            [CanBeNull] IReadOnlyCollection<string> relatableTerms)
        {
            Entity = entity;
            Synonyms = synonyms;
            NearSynonyms = nearSynonyms;
            RelatableTerms = relatableTerms;
        }

        [NotNull]
        public IAnalyzable Entity { get; }

        [CanBeNull]
        public IReadOnlyCollection<string> Synonyms { get; }

        [CanBeNull]
        public IReadOnlyCollection<string> NearSynonyms { get; }

        [CanBeNull]
        public IReadOnlyCollection<string> RelatableTerms { get; }
    }
}