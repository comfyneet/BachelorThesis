using System;
using System.Collections.Generic;
using System.IO;
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
        [NotNull] private readonly IOntologyManager _manager;

        public RetrievalAnalyzer([NotNull] IOntologyManager manager)
        {
            Check.NotNull(manager, nameof(manager));

            _manager = manager;

            Entities = GenerateEntities();

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

            var classEntities = Entities.Where(e => e.Entity is Class).ToList();
            var classes = classEntities.Select(c => (Class) c.Entity).ToList();
            var individualEntities = Entities.Where(e => e.Entity is Individual).ToList();
            var individuals = individualEntities.Select(i => (Individual) i.Entity).ToList();
            var articles = ArticleWeights.Keys.ToList();

            var searchingEntities = new Dictionary<AnalyzableEntity, double>();
            foreach (var term in queryTerms)
            {
                var cleanedTerm = Trim(term);

                var searchableEntities = individualEntities.ToList();
                searchableEntities.AddRange(classEntities);
                foreach (var entity in SearchEntities(searchableEntities, cleanedTerm))
                    if (searchingEntities.ContainsKey(entity.Key))
                    {
                        if (searchingEntities[entity.Key] < entity.Value)
                            searchingEntities[entity.Key] = entity.Value;
                    }
                    else
                    {
                        searchingEntities[entity.Key] = entity.Value;
                    }
            }

            var weights = new Dictionary<Article, double>();
            foreach (var article in articles)
                weights[article] = 0;

            var generatedEntityWeights = GenerateEntityWeights(classes, individuals, searchingEntities);
            foreach (var articleWeights in ArticleWeights)
            {
                var rank = GetRelevanceRank(articleWeights.Value, generatedEntityWeights);
                if (weights[articleWeights.Key] < rank) weights[articleWeights.Key] = rank;
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
                return searchingEntities.Select(e => new KeyValuePair<IAnalyzable, double>(e.Key.Entity, 1.0)).ToList();

            var similarityEntities = new Dictionary<IAnalyzable, double>();
            foreach (var entity in Entities)
            {
                var maxSimilarity = 0.0;

                var terms = new List<string>();
                if (entity.Synonyms != null) terms.AddRange(entity.Synonyms);
                if (entity.NearSynonyms != null) terms.AddRange(entity.NearSynonyms);
                foreach (var t in terms)
                {
                    var similarity = JaroWinkler.Similarity(t, cleanedTerm);
                    if (maxSimilarity < similarity) maxSimilarity = similarity;
                }

                similarityEntities[entity.Entity] = maxSimilarity;
            }

            return similarityEntities.Where(e => e.Value > 0).OrderByDescending(e => e.Value).ToList();
        }

        public IReadOnlyCollection<AnalyzableEntity> Entities { get; }

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

        private IReadOnlyCollection<AnalyzableEntity> GenerateEntities()
        {
            var dictionary = new Dictionary<string, Dictionary<string, double>>();
            var matrixFile = Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Resources\association-matrix.txt");
            if (File.Exists(matrixFile))
                using (var fs = File.OpenRead(matrixFile))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var values = line.Split('\t');
                            var term1 = values[0];
                            var term2 = values[1];
                            var score = double.Parse(values[2]);
                            if (!dictionary.ContainsKey(term1)) dictionary.Add(term1, new Dictionary<string, double>());
                            if (!dictionary.ContainsKey(term2)) dictionary.Add(term2, new Dictionary<string, double>());
                            dictionary[term1][term2] = score;
                            dictionary[term2][term1] = score;
                        }
                    }
                }

            var entities = new List<AnalyzableEntity>();

            foreach (var cls in _manager.GetSubClasses("Thing", OntologyManager.GetType.GetAll))
            {
                var synonyms = new List<string>();
                if (cls.Label != null) synonyms.Add(Trim(cls.Label));

                var relatableTerms = new List<string>();
                foreach (var synonym in synonyms)
                {
                    if (!dictionary.TryGetValue(synonym, out var relations)) continue;

                    var sortedRelations = relations
                        .OrderByDescending(r => r.Value)
                        .Take(5)
                        .Where(r => r.Value > 0)
                        .ToList();

                    foreach (var relation in sortedRelations)
                        if (!synonyms.Contains(relation.Key) && !relatableTerms.Contains(relation.Key))
                            relatableTerms.Add(relation.Key);
                }

                if (relatableTerms.Count == 0) relatableTerms = null;

                entities.Add(new AnalyzableEntity(cls, synonyms, null, relatableTerms));
            }

            foreach (var individual in _manager.GetIndividuals())
            {
                var classTerms = individual.GetAllClasses().Select(c => Trim(c.ToString())).ToList();

                var synonyms = new List<string>();
                if (individual.GetNames() != null)
                    foreach (var name in individual.GetNames())
                    {
                        var trimmedName = Trim(name);
                        string shortTrimmedName = null;
                        foreach (var classTerm in classTerms)
                        {
                            if (!trimmedName.StartsWith(classTerm)) continue;
                            shortTrimmedName = trimmedName.Substring(classTerm.Length).Trim();
                            break;
                        }
                        if (!synonyms.Contains(trimmedName)) synonyms.Add(trimmedName);
                        if (shortTrimmedName != null && !synonyms.Contains(shortTrimmedName))
                            synonyms.Add(shortTrimmedName);
                    }

                var nearSynonyms = new List<string>();
                if (individual.GetTerms() != null)
                    foreach (var term in individual.GetTerms())
                    {
                        var trimmedTerm = Trim(term);
                        string shortTrimmedTerm = null;
                        foreach (var classTerm in classTerms)
                        {
                            if (!trimmedTerm.StartsWith(classTerm)) continue;
                            shortTrimmedTerm = trimmedTerm.Substring(classTerm.Length).Trim();
                            break;
                        }
                        if (!synonyms.Contains(trimmedTerm) && !nearSynonyms.Contains(trimmedTerm))
                            nearSynonyms.Add(trimmedTerm);
                        if (shortTrimmedTerm != null &&
                            !synonyms.Contains(shortTrimmedTerm) &&
                            !nearSynonyms.Contains(trimmedTerm))
                            nearSynonyms.Add(shortTrimmedTerm);
                    }

                var relatableTerms = new List<string>();
                foreach (var synonym in synonyms)
                {
                    if (!dictionary.TryGetValue(synonym, out var relations)) continue;

                    var sortedRelations = relations
                        .OrderByDescending(r => r.Value)
                        .Take(5)
                        .Where(r => r.Value > 0)
                        .ToList();

                    foreach (var relation in sortedRelations)
                        if (!synonyms.Contains(relation.Key) &&
                            !nearSynonyms.Contains(relation.Key)
                            && !relatableTerms.Contains(relation.Key))
                            relatableTerms.Add(relation.Key);
                }

                if (nearSynonyms.Count == 0) nearSynonyms = null;
                if (relatableTerms.Count == 0) relatableTerms = null;

                entities.Add(new AnalyzableEntity(individual, synonyms, nearSynonyms, relatableTerms));
            }

            foreach (var attribute in _manager.GetAttributes())
            {
                var synonyms = new List<string>();
                if (attribute.Label != null) synonyms.Add(Trim(attribute.Label));
                entities.Add(new AnalyzableEntity(attribute, synonyms, null, null));
            }

            foreach (var relation in _manager.GetRelations())
            {
                var synonyms = new List<string>();
                if (relation.Label != null) synonyms.Add(Trim(relation.Label));
                entities.Add(new AnalyzableEntity(relation, synonyms, null, null));
            }

            return entities;
        }

        private void AnalyzeArticles()
        {
            List<Article> articles;
            using (var context = new RiceContext())
            {
                articles = context.Articles.ToList();
            }

            var analyzableEntities = Entities.Where(e => e.Entity is Class || e.Entity is Individual).ToList();

            // Init
            var termCountMaxes = new Dictionary<IAnalyzable, int>();
            foreach (var entity in analyzableEntities) termCountMaxes[entity.Entity] = 0;

            var articleHasTermCounts = new Dictionary<IAnalyzable, int>();
            foreach (var entity in analyzableEntities) articleHasTermCounts[entity.Entity] = 0;

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
                    var terms = new List<string>();
                    if (entity.Synonyms != null) terms.AddRange(entity.Synonyms);
                    if (entity.NearSynonyms != null) terms.AddRange(entity.NearSynonyms);
                    if (entity.RelatableTerms != null) terms.AddRange(entity.RelatableTerms);

                    var orderedTerms = terms.OrderByDescending(v => v).ToList();
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
                    if (count > 0) highestPriorityWeights[article].Add(entity.Entity);

                    count += Regex.Matches(content, pattern).Count;

                    if (count > 0) articleHasTermCounts[entity.Entity]++;
                    articleTermCounts[article].Add(new KeyValuePair<IAnalyzable, int>(entity.Entity, count));
                    if (termCountMaxes[entity.Entity] < count) termCountMaxes[entity.Entity] = count;
                }
            }

            var termIdfs = new Dictionary<IAnalyzable, double>();
            foreach (var entity in analyzableEntities)
                if (articleHasTermCounts[entity.Entity] != 0)
                    termIdfs[entity.Entity] = Math.Log((double) articles.Count / articleHasTermCounts[entity.Entity]);
                else termIdfs[entity.Entity] = 0;

            var maxIdf = termIdfs.Max(term => term.Value);
            if (maxIdf > 0)
                foreach (var entity in analyzableEntities)
                    termIdfs[entity.Entity] = termIdfs[entity.Entity] / maxIdf;

            var articleTfs = new Dictionary<Article, IList<KeyValuePair<IAnalyzable, double>>>();
            foreach (var articleTermCount in articleTermCounts)
            {
                articleTfs[articleTermCount.Key] = new List<KeyValuePair<IAnalyzable, double>>();
                foreach (var entity in analyzableEntities)
                {
                    var f_t_d = articleTermCount.Value.First(pair => pair.Key.Equals(entity.Entity)).Value;
                    double tf = 0;
                    if (termCountMaxes[entity.Entity] != 0) tf = (double) f_t_d / termCountMaxes[entity.Entity];
                    articleTfs[articleTermCount.Key].Add(new KeyValuePair<IAnalyzable, double>(entity.Entity, tf));
                }
            }

            var weights = new Dictionary<Article, IReadOnlyDictionary<IAnalyzable, double>>();
            foreach (var article in articles)
            {
                var tfIdf = new Dictionary<IAnalyzable, double>();
                foreach (var entity in analyzableEntities)
                    if (highestPriorityWeights[article].Contains(entity.Entity)) tfIdf[entity.Entity] = 1.0;
                    else
                        tfIdf[entity.Entity] = articleTfs[article]
                                                   .First(pair => pair.Key.Equals(entity.Entity)).Value *
                                               termIdfs[entity.Entity];

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
        private IReadOnlyDictionary<AnalyzableEntity, double> SearchEntities(
            [NotNull] IReadOnlyCollection<AnalyzableEntity> entities,
            [NotNull] string term)
        {
            Check.NotNull(entities, nameof(term));
            Check.NotEmpty(term, nameof(term));

            var results = new Dictionary<AnalyzableEntity, double>();
            foreach (var entity in entities)
            {
                if (entity.Synonyms != null && entity.Synonyms.Contains(term))
                {
                    results[entity] = 1.0;
                    continue;
                }
                if (entity.NearSynonyms != null && entity.NearSynonyms.Contains(term))
                {
                    results[entity] = 0.8;
                    continue;
                }

                if (entity.RelatableTerms != null && entity.RelatableTerms.Contains(term))
                    results[entity] = 0.4;
            }

            return results;
        }

        [NotNull]
        private IReadOnlyDictionary<IAnalyzable, double> GenerateEntityWeights(
            [NotNull] IReadOnlyCollection<Class> classes,
            [NotNull] IReadOnlyCollection<Individual> individuals,
            [NotNull] IReadOnlyDictionary<AnalyzableEntity, double> entities)
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
                if (entity.Key.Entity is Class cls) GenerateSubClassWeights(cls, 1.0 * entity.Value);
                else weights[entity.Key.Entity] = 1.0 * entity.Value;

            return weights;
        }
    }
}