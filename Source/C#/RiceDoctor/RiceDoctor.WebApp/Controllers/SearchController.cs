using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
using RiceDoctor.QueryAnalysis;
using RiceDoctor.RetrievalAnalysis;
using RiceDoctor.Shared;
using RiceDoctor.WebApp.Models;
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
            IReadOnlyDictionary<IAnalyzable, double> SearchClasses(string term)
            {
                var classes = _ontologyManager.GetSubClasses("Thing", GetAll);
                var tmpResultClasses = new Dictionary<Class, double>();
                foreach (var cls in classes)
                {
                    var distance = 0.0;
                    if (cls.Label != null)
                        distance = DiceCoefficient.Distance(cls.Label.ToLower().RemoveAccents(), term);
                    var idDistance = DiceCoefficient.Distance(cls.Id.ToLower().RemoveAccents(), term);
                    if (idDistance > distance) distance = idDistance;
                    if (distance > 0) tmpResultClasses.Add(cls, distance);
                }

                var resultClasses = tmpResultClasses
                    .OrderByDescending(c => c.Value)
                    .ToDictionary(c => (IAnalyzable) c.Key, c => c.Value);
                return resultClasses.Count == 0 ? null : resultClasses;
            }

            IReadOnlyDictionary<IAnalyzable, double> SearchIndividuals(string term)
            {
                var individuals = _ontologyManager.GetIndividuals();
                var tmpResultIndividuals = new Dictionary<Individual, double>();
                foreach (var individual in individuals)
                {
                    var distance = 0.0;
                    var names = individual.GetNames();
                    if (names != null)
                        distance = names
                            .Select(name => DiceCoefficient.Distance(name.ToLower().RemoveAccents(), term))
                            .Concat(new[] {distance})
                            .Max();
                    var idDistance = DiceCoefficient.Distance(individual.Id.ToLower(), term);
                    if (idDistance > distance) distance = idDistance;
                    if (distance > 0) tmpResultIndividuals.Add(individual, distance);
                }

                var resultIndividuals = tmpResultIndividuals
                    .OrderByDescending(i => i.Value)
                    .ToDictionary(i => (IAnalyzable) i.Key, i => i.Value);
                return resultIndividuals.Count == 0 ? null : resultIndividuals;
            }

            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Home");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;

            var results =
                new Dictionary<string, List<KeyValuePair<SearchableType, IReadOnlyDictionary<IAnalyzable, double>>>>();

            foreach (var query in _queryAnalyzer.Queries)
            {
                var terms = query.Match(keywords);
                if (terms == null) continue;

                foreach (var term in terms)
                {
                    if (!results.ContainsKey(term))
                        results.Add(term,
                            new List<KeyValuePair<SearchableType, IReadOnlyDictionary<IAnalyzable, double>>>());

                    results[term]
                        .Add(new KeyValuePair<SearchableType, IReadOnlyDictionary<IAnalyzable, double>>(
                            SearchableType.Class, SearchClasses(term)));

                    results[term]
                        .Add(new KeyValuePair<SearchableType, IReadOnlyDictionary<IAnalyzable, double>>(
                            SearchableType.Individual, SearchIndividuals(term)));
                }

                break;
            }

            return results.Count == 0 ? View(null) : View(results);
        }
    }
}