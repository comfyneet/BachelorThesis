using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;

namespace RiceDoctor.RetrievalAnalysis
{
    public class RetrievalAnalyzer : IRetrievalAnalyzer
    {
        [NotNull] private readonly IReadOnlyDictionary<IAnalyzable, IReadOnlyList<string>> _entities;

        public RetrievalAnalyzer(
            [NotNull] IReadOnlyCollection<Class> classes,
            [NotNull] IReadOnlyCollection<Individual> individuals)
        {
            Check.NotNull(classes, nameof(classes));
            Check.NotNull(individuals, nameof(individuals));

            var entities = new Dictionary<IAnalyzable, IReadOnlyList<string>>();

            foreach (var cls in classes)
                // hack
                if (cls.Label != null)
                    entities.Add(cls, new List<string> {Trim(cls.Label)});

            foreach (var individual in individuals)
            {
                var terms = new List<string>();

                var names = individual.GetNames();
                if (names != null) terms.AddRange(names.Select(Trim));

                var individualTerms = individual.GetTerms();
                if (individualTerms != null) terms.AddRange(individualTerms.Select(Trim));

                if (terms.Count > 0)
                    entities.Add(individual, terms);
            }

            _entities = entities;
        }

        public IReadOnlyDictionary<Article, IReadOnlyDictionary<IAnalyzable, double>>
            AnalyzeArticles(IReadOnlyCollection<Article> articles)
        {
            Check.NotNull(articles, nameof(articles));

            // Init
            var termCountMaxes = new Dictionary<IAnalyzable, int>();
            foreach (var pair in _entities) termCountMaxes[pair.Key] = 0;

            var articleHasTermCounts = new Dictionary<IAnalyzable, int>();
            foreach (var entity in _entities) articleHasTermCounts[entity.Key] = 0;

            var highestPriorityWeights = new Dictionary<Article, IList<IAnalyzable>>();
            var articleTermCounts = new Dictionary<Article, IList<KeyValuePair<IAnalyzable, int>>>();
            foreach (var article in articles)
            {
                highestPriorityWeights[article] = new List<IAnalyzable>();
                articleTermCounts[article] = new List<KeyValuePair<IAnalyzable, int>>();
            }
            // End Init

            foreach (var article in articles)
            {
                var title = article.Title == "" ? "" : Trim(article.Title);
                var content = article.Content == "" ? "" : Trim(article.Content);

                foreach (var entity in _entities)
                {
                    var orderedTerms = entity.Value.OrderByDescending(v => v).ToList();
                    var patternBuilder = new StringBuilder();
                    for (var i = 0; i < orderedTerms.Count; ++i)
                    {
                        if (i > 0) patternBuilder.Append('|');
                        patternBuilder.Append(@"(?<!\S)");
                        patternBuilder.Append(orderedTerms[i]);
                        patternBuilder.Append(@"(?![^\s])");
                    }
                    var pattern = patternBuilder.ToString();

                    var count = Regex.Matches(title, pattern).Count;
                    if (count > 0) highestPriorityWeights[article].Add(entity.Key);

                    count += Regex.Matches(content, pattern).Count;

                    if (count > 0) articleHasTermCounts[entity.Key]++;
                    articleTermCounts[article].Add(new KeyValuePair<IAnalyzable, int>(entity.Key, count));
                    if (termCountMaxes[entity.Key] < count) termCountMaxes[entity.Key] = count;
                }
            }

            var termIdfs = new Dictionary<IAnalyzable, double>();
            foreach (var entity in _entities)
                if (articleHasTermCounts[entity.Key] != 0)
                    termIdfs[entity.Key] = Math.Log((double) articles.Count / articleHasTermCounts[entity.Key]);
                else termIdfs[entity.Key] = 0;

            var articleTfs = new Dictionary<Article, IList<KeyValuePair<IAnalyzable, double>>>();
            foreach (var articleTermCount in articleTermCounts)
            {
                articleTfs[articleTermCount.Key] = new List<KeyValuePair<IAnalyzable, double>>();
                foreach (var entity in _entities)
                {
                    var f_t_d = articleTermCount.Value.First(pair => pair.Key == entity.Key).Value;
                    double tf = 0;
                    if (termCountMaxes[entity.Key] != 0) tf = 0.5 + 0.5 * f_t_d / termCountMaxes[entity.Key];
                    articleTfs[articleTermCount.Key].Add(new KeyValuePair<IAnalyzable, double>(entity.Key, tf));
                }
            }

            var weights = new Dictionary<Article, IReadOnlyDictionary<IAnalyzable, double>>();
            foreach (var article in articles)
            {
                var tfIdf = new Dictionary<IAnalyzable, double>();
                foreach (var entity in _entities)
                    if (highestPriorityWeights[article].Contains(entity.Key)) tfIdf[entity.Key] = 1.0;
                    else
                        tfIdf[entity.Key] = articleTfs[article]
                                                .First(pair => pair.Key == entity.Key).Value * termIdfs[entity.Key];

                weights[article] = tfIdf;
            }

            return weights;
        }

        public double GetRelevanceRank(
            IReadOnlyDictionary<IAnalyzable, double> articleWeights,
            IReadOnlyDictionary<IAnalyzable, double> queryWeights)
        {
            Check.NotNull(articleWeights, nameof(articleWeights));
            Check.NotNull(queryWeights, nameof(queryWeights));

            if (articleWeights.Count != queryWeights.Count) throw new ArgumentException(nameof(GetRelevanceRank));

            var articleNorm = 0.0;
            foreach (var articleWeight in articleWeights)
                articleNorm += Math.Pow(articleWeight.Value, 2);
            articleNorm = Math.Sqrt(articleNorm);

            var queryNorm = 0.0;
            foreach (var queryWeight in queryWeights)
                queryNorm += Math.Pow(queryWeight.Value, 2);
            queryNorm = Math.Sqrt(queryNorm);

            var dotProduct = 0.0;
            foreach (var articleWeight in articleWeights)
                dotProduct += articleWeight.Value * queryWeights[articleWeight.Key];

            var rank = dotProduct / (articleNorm * queryNorm);

            return rank;
        }

        public IReadOnlyList<IAnalyzable> Entities => _entities.Keys.ToList();

        [NotNull]
        private string Trim([NotNull] string text)
        {
            Check.NotEmpty(text, nameof(text));

            return text.ToLower().RemoveNonWordChars();
        }
    }
}