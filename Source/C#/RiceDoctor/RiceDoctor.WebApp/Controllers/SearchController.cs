using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.InformationRetrieval;
using RiceDoctor.OntologyManager;
using RiceDoctor.QueryManager;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.WebApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly IOntologyManager _ontologyManager;
        private readonly IQueryManager _queryManager;

        public SearchController(
            [FromServices] [NotNull] IOntologyManager ontologyManager,
            [FromServices] [NotNull] IQueryManager queryManager)
        {
            Check.NotNull(ontologyManager, nameof(ontologyManager));
            Check.NotNull(queryManager, nameof(queryManager));

            _ontologyManager = ontologyManager;
            _queryManager = queryManager;
        }

        public IActionResult Index(string keywords)
        {
            IReadOnlyDictionary<object, double> SearchClasses(string term)
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
                    .ToDictionary(c => (object) c.Key, c => c.Value);
                return resultClasses.Count == 0 ? null : resultClasses;
            }

            IReadOnlyDictionary<object, double> SearchIndividuals(string term)
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
                    .ToDictionary(i => (object) i.Key, i => i.Value);
                return resultIndividuals.Count == 0 ? null : resultIndividuals;
            }

            IReadOnlyDictionary<object, double> SearchRelations(string term)
            {
                var relations = _ontologyManager.GetRelations();
                var tmpRelationResults = new Dictionary<Relation, double>();
                foreach (var relation in relations)
                {
                    var distance = 0.0;
                    if (relation.Label != null)
                        distance = DiceCoefficient.Distance(relation.Label.ToLower().RemoveAccents(), term);
                    var idDistance = DiceCoefficient.Distance(relation.Id.ToLower().RemoveAccents(), term);
                    if (idDistance > distance) distance = idDistance;
                    if (distance > 0) tmpRelationResults.Add(relation, distance);
                }

                var resultRelations = tmpRelationResults
                    .OrderByDescending(r => r.Value)
                    .ToDictionary(r => (object) r.Key, r => r.Value);
                return resultRelations.Count == 0 ? null : resultRelations;
            }

            IReadOnlyDictionary<object, double> SearchAttributes(string term)
            {
                var attributes = _ontologyManager.GetAttributes();
                var tmpAttributeResults = new Dictionary<Attribute, double>();
                foreach (var attribute in attributes)
                {
                    var distance = 0.0;
                    if (attribute.Label != null)
                        distance = DiceCoefficient.Distance(attribute.Label.ToLower().RemoveAccents(), term);
                    var idDistance = DiceCoefficient.Distance(attribute.Id.ToLower().RemoveAccents(), term);
                    if (idDistance > distance) distance = idDistance;
                    if (distance > 0) tmpAttributeResults.Add(attribute, distance);
                }

                var resultAttributes = tmpAttributeResults
                    .OrderByDescending(a => a.Value)
                    .ToDictionary(a => (object) a.Key, a => a.Value);
                return resultAttributes.Count == 0 ? null : resultAttributes;
            }

            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Home");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;

            var results = new Dictionary<string, List<KeyValuePair<QueryType, IReadOnlyDictionary<object, double>>>>();

            foreach (var query in _queryManager.Queries)
            {
                var terms = query.Match(keywords);
                if (terms == null) continue;

                for (var i = 0; i < terms.Count; ++i)
                {
                    if (!results.ContainsKey(terms[i]))
                        results.Add(terms[i], new List<KeyValuePair<QueryType, IReadOnlyDictionary<object, double>>>());

                    var resultTypes = query.ResultTypes[i];
                    foreach (var resultType in resultTypes)
                        switch (resultType)
                        {
                            case QueryType.Class:
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyDictionary<object, double>>(
                                        QueryType.Class,
                                        SearchClasses(terms[i])));
                                break;

                            case QueryType.Individual:
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyDictionary<object, double>>(
                                        QueryType.Individual,
                                        SearchIndividuals(terms[i])));
                                break;

                            case QueryType.Relation:
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyDictionary<object, double>>(
                                        QueryType.Relation,
                                        SearchRelations(terms[i])));
                                break;

                            case QueryType.Attribute:
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyDictionary<object, double>>(
                                        QueryType.Attribute,
                                        SearchAttributes(terms[i])));
                                break;
                        }
                }

                break;
            }

            return results.Count == 0 ? View(null) : View(results);
        }
    }
}