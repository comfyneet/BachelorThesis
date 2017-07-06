using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;

namespace RiceDoctor.RetrievalAnalysis
{
    public interface IRetrievalAnalyzer
    {
        [NotNull]
        IReadOnlyList<IAnalyzable> Entities { get; }

        [NotNull]
        IReadOnlyDictionary<Article, IReadOnlyDictionary<IAnalyzable, double>>
            AnalyzeArticles([NotNull] IReadOnlyCollection<Article> articles);

        double GetRelevanceRank(
            [NotNull] IReadOnlyDictionary<IAnalyzable, double> articleWeights,
            [NotNull] IReadOnlyDictionary<IAnalyzable, double> queryWeights);
    }
}