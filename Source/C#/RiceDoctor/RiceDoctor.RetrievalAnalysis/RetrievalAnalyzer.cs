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
        public RetrievalAnalyzer([NotNull] IOntologyManager manager)
        {
            Check.NotNull(manager, nameof(manager));

            var entities = new Dictionary<IAnalyzable, IReadOnlyList<string>>();

            foreach (var cls in manager.GetSubClasses("Thing", OntologyManager.GetType.GetAll))
            {
                var terms = new List<string> {Trim(cls.Id)};
                if (cls.Label != null) terms.Add(Trim(cls.Label));
                entities.Add(cls, terms);
            }

            foreach (var individual in manager.GetIndividuals())
            {
                var terms = new List<string> {Trim(individual.Id)};
                if (individual.GetNames() != null) terms.AddRange(individual.GetNames().Select(Trim));
                if (individual.GetTerms() != null) terms.AddRange(individual.GetTerms().Select(Trim));

                entities.Add(individual, terms);
            }

            foreach (var attribute in manager.GetAttributes())
            {
                var terms = new List<string> {Trim(attribute.Id)};
                if (attribute.Label != null) terms.Add(Trim(attribute.Label));
                entities.Add(attribute, terms);
            }

            foreach (var relation in manager.GetRelations())
            {
                var terms = new List<string> {Trim(relation.Id)};
                if (relation.Label != null) terms.Add(Trim(relation.Label));
                entities.Add(relation, terms);
            }

            Entities = entities;
            AnalyzeArticles();
        }

        public IReadOnlyCollection<KeyValuePair<Article, double>> AnalyzeRelevanceRank(
            IReadOnlyCollection<string> queryTerms)
        {
            Check.NotNull(queryTerms, nameof(queryTerms));

            if (RequireUpdateWeights)
            {
                AnalyzeArticles();
                RequireUpdateWeights = false;
            }

            var classEntities = Entities.Where(e => e.Key is Class).ToList();
            var classes = classEntities.Select(c => (Class) c.Key).ToList();
            var individualEntities = Entities.Where(e => e.Key is Individual).ToList();
            var individuals = individualEntities.Select(i => (Individual) i.Key).ToList();
            var articles = ArticleWeights.Keys.ToList();

            var searchingEntities = new List<IAnalyzable>();
            foreach (var term in queryTerms)
            {
                var cleanedTerm = term.ToLower().RemoveNonWordChars();
                foreach (var individual in SearchEntities(individualEntities, cleanedTerm))
                    searchingEntities.Add(individual);
                foreach (var cls in SearchEntities(classEntities, cleanedTerm))
                    searchingEntities.Add(cls);
            }

            var weights = new Dictionary<Article, double>();
            foreach (var article in articles)
                weights[article] = 0;

            foreach (var entitySubset in searchingEntities.GetSubsets())
            {
                var generatedEntityWeights = GenerateEntityWeights(classes, individuals, entitySubset);
                foreach (var articleWeights in ArticleWeights)
                {
                    var rank = GetRelevanceRank(articleWeights.Value, generatedEntityWeights);
                    if (weights[articleWeights.Key] < rank) weights[articleWeights.Key] = rank;
                }
            }

            var result = weights.Where(w => w.Value > 0).OrderByDescending(w => w.Value).ToList();
            return result.Count == 0 ? null : result;
        }

        public IReadOnlyCollection<KeyValuePair<IAnalyzable, double>> FindOntologyEntity(string term)
        {
            Check.NotEmpty(term, nameof(term));

            var cleanedTerm = Trim(term);
            var searchingEntities = SearchEntities(Entities, cleanedTerm);
            if (searchingEntities.Count > 0)
                return searchingEntities.Select(e => new KeyValuePair<IAnalyzable, double>(e, 1.0)).ToList();

            var similarityEntities = new Dictionary<IAnalyzable, double>();
            foreach (var entity in Entities)
            {
                var maxSimilarity = 0.0;
                foreach (var t in entity.Value)
                {
                    var similarity = JaroWinkler.Similarity(t, cleanedTerm);
                    if (maxSimilarity < similarity) maxSimilarity = similarity;
                }

                similarityEntities[entity.Key] = maxSimilarity;
            }

            return similarityEntities.Where(e => e.Value > 0).OrderByDescending(e => e.Value).ToList();
        }

        public IReadOnlyDictionary<IAnalyzable, IReadOnlyList<string>> Entities { get; }

        public IReadOnlyDictionary<Article, IReadOnlyDictionary<IAnalyzable, double>> ArticleWeights
        {
            get;
            private set;
        }

        public bool RequireUpdateWeights { get; set; }

        [NotNull]
        private string Trim([NotNull] string text)
        {
            Check.NotEmpty(text, nameof(text));

            return text.ToLower().RemoveNonWordChars();
        }

        private void AnalyzeArticles()
        {
            List<Article> articles;
            using (var context = new RiceContext())
            {
                articles = context.Articles.ToList();
            }

            var analyzableEntities = Entities.Where(e => e.Key is Class || e.Key is Individual).ToList();

            // Init
            var termCountMaxes = new Dictionary<IAnalyzable, int>();
            foreach (var pair in analyzableEntities) termCountMaxes[pair.Key] = 0;

            var articleHasTermCounts = new Dictionary<IAnalyzable, int>();
            foreach (var entity in analyzableEntities) articleHasTermCounts[entity.Key] = 0;

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

                foreach (var entity in analyzableEntities)
                {
                    var orderedTerms = entity.Value.OrderByDescending(v => v).ToList();
                    var patternBuilder = new StringBuilder();
                    patternBuilder.Append(@"\b(");
                    for (var i = 0; i < orderedTerms.Count; ++i)
                    {
                        if (i > 0) patternBuilder.Append('|');
                        patternBuilder.Append(orderedTerms[i]);
                    }
                    patternBuilder.Append(@")\b");
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
            foreach (var entity in analyzableEntities)
                if (articleHasTermCounts[entity.Key] != 0)
                    termIdfs[entity.Key] = Math.Log((double) articles.Count / articleHasTermCounts[entity.Key]);
                else termIdfs[entity.Key] = 0;

            var maxIdf = termIdfs.Max(term => term.Value);
            if (maxIdf > 0)
                foreach (var entity in analyzableEntities)
                    termIdfs[entity.Key] = termIdfs[entity.Key] / maxIdf;

            var articleTfs = new Dictionary<Article, IList<KeyValuePair<IAnalyzable, double>>>();
            foreach (var articleTermCount in articleTermCounts)
            {
                articleTfs[articleTermCount.Key] = new List<KeyValuePair<IAnalyzable, double>>();
                foreach (var entity in analyzableEntities)
                {
                    var f_t_d = articleTermCount.Value.First(pair => pair.Key.Equals(entity.Key)).Value;
                    double tf = 0;
                    if (termCountMaxes[entity.Key] != 0) tf = (double) f_t_d / termCountMaxes[entity.Key];
                    articleTfs[articleTermCount.Key].Add(new KeyValuePair<IAnalyzable, double>(entity.Key, tf));
                }
            }

            var weights = new Dictionary<Article, IReadOnlyDictionary<IAnalyzable, double>>();
            foreach (var article in articles)
            {
                var tfIdf = new Dictionary<IAnalyzable, double>();
                foreach (var entity in analyzableEntities)
                    if (highestPriorityWeights[article].Contains(entity.Key)) tfIdf[entity.Key] = 1.0;
                    else
                        tfIdf[entity.Key] = articleTfs[article]
                                                .First(pair => pair.Key == entity.Key).Value * termIdfs[entity.Key];

                weights[article] = tfIdf;
            }

            ArticleWeights = weights;
        }

        private double GetRelevanceRank(
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

        [NotNull]
        private IReadOnlyCollection<IAnalyzable> SearchEntities(
            [NotNull] IReadOnlyCollection<KeyValuePair<IAnalyzable, IReadOnlyList<string>>> entities,
            [NotNull] string term)
        {
            Check.NotNull(entities, nameof(term));
            Check.NotEmpty(term, nameof(term));

            var results = new List<IAnalyzable>();
            foreach (var entity in entities)
                if (entity.Value.Contains(term))
                    results.Add(entity.Key);

            return results;
        }

        [NotNull]
        private IReadOnlyDictionary<IAnalyzable, double> GenerateEntityWeights(
            [NotNull] IReadOnlyCollection<Class> classes,
            [NotNull] IReadOnlyCollection<Individual> individuals,
            [NotNull] IReadOnlyCollection<IAnalyzable> entities)
        {
            Check.NotNull(classes, nameof(classes));
            Check.NotNull(individuals, nameof(individuals));
            Check.NotNull(entities, nameof(entities));

            var weights = new Dictionary<IAnalyzable, double>();

            void GenerateSubClassWeights(Class cls, double weight)
            {
                if (weights[cls] < weight) weights[cls] = weight;

                if (cls.GetDirectIndividuals() != null)
                    foreach (var directIndividual in cls.GetDirectIndividuals())
                    {
                        var possibleWeight = weight * 0.5;
                        if (weights[directIndividual] < possibleWeight) weights[directIndividual] = possibleWeight;
                    }

                if (cls.GetDirectSubClasses() != null)
                    foreach (var subClass in cls.GetDirectSubClasses())
                    {
                        var possibleWeight = weight * 0.7;
                        if (weights[subClass] < possibleWeight) weights[subClass] = possibleWeight;
                        GenerateSubClassWeights(subClass, weight * 0.9);
                    }
            }

            foreach (var cls in classes) weights[cls] = 0;
            foreach (var individual in individuals) weights[individual] = 0;

            foreach (var entity in entities)
                if (entity is Class cls) GenerateSubClassWeights(cls, 1.0);
                else weights[entity] = 1.0;

            return weights;
        }
    }
}