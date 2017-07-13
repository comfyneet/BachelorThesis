using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;

namespace RiceDoctor.RetrievalAnalysis
{
    public interface IRetrievalAnalyzer
    {
        [NotNull]
        IReadOnlyDictionary<IAnalyzable, IReadOnlyList<string>> Entities { get; }

        [NotNull]
        IReadOnlyDictionary<Article, IReadOnlyDictionary<IAnalyzable, double>> ArticleWeights { get; }

        bool RequireUpdateWeights { get; set; }

        [CanBeNull]
        IReadOnlyCollection<KeyValuePair<Article, double>> AnalyzeRelevanceRank(
            [NotNull] IReadOnlyCollection<string> queryTerms);

        [NotNull]
        IReadOnlyCollection<KeyValuePair<IAnalyzable, double>> FindOntologyEntity([NotNull] string term);
    }
}