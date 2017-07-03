using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;

namespace RiceDoctor.RetrievalAnalysis
{
    public interface IRetrievalAnalyzer
    {
        [NotNull]
        IReadOnlyDictionary<Article, IReadOnlyDictionary<IAnalyzable, double>>
            AnalyzeArticles([NotNull] IReadOnlyCollection<Article> articles);
    }
}