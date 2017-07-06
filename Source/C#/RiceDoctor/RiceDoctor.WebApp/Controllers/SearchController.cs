using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.QueryAnalysis;
using RiceDoctor.RetrievalAnalysis;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.WebApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly IOntologyManager _ontologyManager;
        private readonly IQueryAnalyzer _queryAnalyzer;

        public SearchController(
            [FromServices] [NotNull] IOntologyManager ontologyManager,
            [FromServices] [NotNull] IQueryAnalyzer queryAnalyzer)
        {
            Check.NotNull(ontologyManager, nameof(ontologyManager));
            Check.NotNull(queryAnalyzer, nameof(queryAnalyzer));

            _ontologyManager = ontologyManager;
            _queryAnalyzer = queryAnalyzer;
        }

        public IActionResult Index(string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Home");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;

            var ontologyClasses = _ontologyManager.GetSubClasses("Thing", GetAll);
            var ontologyIndividuals = _ontologyManager.GetIndividuals();

            List<Article> articles;
            using (var context = new RiceContext())
            {
                articles = context.Articles.ToList();
            }
            var analyzer = new RetrievalAnalyzer(ontologyClasses, ontologyIndividuals);
            ontologyClasses = analyzer.Entities.OfType<Class>().ToList();
            ontologyIndividuals = analyzer.Entities.OfType<Individual>().ToList();
            var articleWeightList = analyzer.AnalyzeArticles(articles);

            List<KeyValuePair<Article, double>> results = null;
            foreach (var query in _queryAnalyzer.Queries)
            {
                var terms = query.Match(keywords);
                if (terms == null) continue;

                var searchingEntities = new List<IAnalyzable>();
                foreach (var term in terms)
                {
                    var cleanedTerm = term.RemoveNonWordChars();
                    foreach (var individual in SearchIndividuals(ontologyIndividuals, cleanedTerm))
                        if (!searchingEntities.Contains(individual))
                            searchingEntities.Add(individual);
                    foreach (var cls in SearchClasses(ontologyClasses, cleanedTerm))
                        if (!searchingEntities.Contains(cls))
                            searchingEntities.Add(cls);
                }

                var weights = new Dictionary<Article, double>();
                foreach (var article in articles)
                    weights[article] = 0;

                foreach (var entitySubset in searchingEntities.GetSubsets())
                {
                    var generatedEntityWeights =
                        GenerateEntityWeights(ontologyClasses, ontologyIndividuals, entitySubset);
                    foreach (var articleWeights in articleWeightList)
                    {
                        var rank = analyzer.GetRelevanceRank(articleWeights.Value, generatedEntityWeights);
                        if (weights[articleWeights.Key] < rank) weights[articleWeights.Key] = rank;
                    }
                }

                results = weights.Where(w => w.Value > 0).ToList().OrderByDescending(w => w.Value).ToList();

                break;
            }

            return results == null ? View(null) : View(results);
        }

        [NotNull]
        private IReadOnlyCollection<Class> SearchClasses(
            [NotNull] IReadOnlyCollection<Class> ontologyClasses,
            [NotNull] string cleanedTerm)
        {
            Check.NotNull(ontologyClasses, nameof(ontologyClasses));
            Check.NotEmpty(cleanedTerm, nameof(cleanedTerm));

            var results = new List<Class>();
            foreach (var cls in ontologyClasses)
            {
                if (cls.Label == null) continue;
                var label = cls.Label.ToLower().RemoveNonWordChars();
                if (label == cleanedTerm && !results.Contains(cls)) results.Add(cls);
            }

            return results;
        }

        [NotNull]
        private IReadOnlyCollection<Individual> SearchIndividuals(
            [NotNull] IReadOnlyCollection<Individual> ontologyIndividuals,
            [NotNull] string cleanedTerm)
        {
            Check.NotNull(ontologyIndividuals, nameof(ontologyIndividuals));
            Check.NotEmpty(cleanedTerm, nameof(cleanedTerm));

            var results = new List<Individual>();

            foreach (var individual in ontologyIndividuals)
            {
                var canAdd = false;
                if (individual.GetNames() != null)
                    if (individual.GetNames().Any(name => name == cleanedTerm))
                        canAdd = true;

                if (!canAdd && individual.GetTerms() != null)
                    if (individual.GetTerms().Any(term => term == cleanedTerm))
                        canAdd = true;

                if (canAdd && !results.Contains(individual))
                    results.Add(individual);
            }

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
            foreach (var cls in classes)
                weights[cls] = 0;
            foreach (var individual in individuals)
                weights[individual] = 0;

            foreach (var entity in entities)
            {
                weights[entity] = 1.0;
                if (entity is Class cls)
                {
                    if (cls.GetAllSubClasses() != null)
                        foreach (var subClass in cls.GetAllSubClasses())
                            if (weights[entity] < 0.8) weights[subClass] = 0.8;
                    if (cls.GetDirectIndividuals() != null)
                        foreach (var directIndividual in cls.GetDirectIndividuals())
                            if (weights[directIndividual] < 0.7) weights[directIndividual] = 0.7;
                }
                else
                {
                    var individual = (Individual) entity;
                    var directClass = individual.GetDirectClass();
                    if (weights[directClass] < 0.5) weights[directClass] = 0.5;
                }
            }

            return weights;
        }
    }
}