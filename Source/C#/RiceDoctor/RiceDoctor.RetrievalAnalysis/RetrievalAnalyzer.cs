using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;

namespace RiceDoctor.RetrievalAnalysis
{
    public class RetrievalAnalyzer : IRetrievalAnalyzer
    {
        [NotNull] private readonly IReadOnlyDictionary<IAnalyzable, IReadOnlyCollection<string>> _entities;

        public RetrievalAnalyzer()
        {
            var entities = new Dictionary<IAnalyzable, IReadOnlyCollection<string>>();

            foreach (var cls in Manager.Instance.GetClass(Manager.Instance.ThingClassId).GetAllSubClasses())
                // hack
                if (cls.Label != null)
                    entities.Add(cls, new List<string> {Trim(cls.Label)});

            foreach (var individual in Manager.Instance.GetIndividuals())
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

            var termCountMaxes = new Dictionary<IAnalyzable, int>();
            foreach (var entity in _entities)
                termCountMaxes[entity.Key] = 0;

            var articleHasTermCounts = new Dictionary<IAnalyzable, int>();
            var articleTermCounts = new Dictionary<Article, IList<KeyValuePair<IAnalyzable, int>>>();
            foreach (var article in articles)
            {
                var title = Trim(article.Title);
                var content = Trim(article.Content);

                articleTermCounts[article] = new List<KeyValuePair<IAnalyzable, int>>();
                foreach (var entity in _entities)
                {
                    var count = 0;
                    foreach (var term in entity.Value)
                    {
                        count += Regex.Matches(Regex.Escape(title), term).Count;
                        count += Regex.Matches(Regex.Escape(content), term).Count;
                    }

                    if (!articleHasTermCounts.ContainsKey(entity.Key)) articleHasTermCounts[entity.Key] = 0;
                    if (count > 0) articleHasTermCounts[entity.Key]++;
                    articleTermCounts[article].Add(new KeyValuePair<IAnalyzable, int>(entity.Key, count));
                    if (termCountMaxes[entity.Key] < count) termCountMaxes[entity.Key] = count;
                }
            }

            var termIdfs = new Dictionary<IAnalyzable, double>();
            foreach (var entity in _entities)
                // hack
                if (articleHasTermCounts[entity.Key] != 0)
                    termIdfs[entity.Key] = Math.Log((double) articles.Count / articleHasTermCounts[entity.Key]);
                else termIdfs[entity.Key] = 0;

            var articleTfs = new Dictionary<Article, IList<KeyValuePair<IAnalyzable, double>>>();
            foreach (var articleTermCount in articleTermCounts)
            {
                articleTfs[articleTermCount.Key] = new List<KeyValuePair<IAnalyzable, double>>();
                foreach (var entity in _entities)
                {
                    var n = articleTermCount.Value.First(pair => pair.Key == entity.Key).Value;
                    double tf = 0; // hack
                    if (termCountMaxes[entity.Key] != 0) tf = (double) n / termCountMaxes[entity.Key];
                    articleTfs[articleTermCount.Key].Add(new KeyValuePair<IAnalyzable, double>(entity.Key, tf));
                }
            }

            var weights = new Dictionary<Article, IReadOnlyDictionary<IAnalyzable, double>>();
            foreach (var article in articles)
            {
                var tfIdf = new Dictionary<IAnalyzable, double>();
                foreach (var entity in _entities)
                    tfIdf[entity.Key] =
                        articleTfs[article].First(pair => pair.Key == entity.Key).Value * termIdfs[entity.Key];

                weights[article] = tfIdf;
            }

            return weights;
        }

        [NotNull]
        private string Trim([NotNull] string text)
        {
            Check.NotEmpty(text, nameof(text));

            return Regex.Replace(text.ToLower().RemoveAccents().Trim(), @"\s+", " ");
        }
    }
}