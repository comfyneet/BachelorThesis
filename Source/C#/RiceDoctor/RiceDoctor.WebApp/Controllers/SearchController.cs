using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
using RiceDoctor.QueryManager;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;
using Attribute = RiceDoctor.OntologyManager.Attribute;

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
            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Home");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;

            var results = new Dictionary<string, List<KeyValuePair<QueryType, IReadOnlyCollection<object>>>>();

            foreach (var query in _queryManager.Queries)
            {
                var terms = query.Match(keywords);
                if (terms == null) continue;

                for (var i = 0; i < terms.Count; ++i)
                {
                    if (!results.ContainsKey(terms[i]))
                        results.Add(terms[i], new List<KeyValuePair<QueryType, IReadOnlyCollection<object>>>());

                    var resultTypes = query.ResultTypes[i];
                    foreach (var resultType in resultTypes)
                        switch (resultType)
                        {
                            case QueryType.Class:
                                var classes = _ontologyManager.GetSubClasses("Thing", GetAll);
                                var resultClasses = classes
                                    .Where(cls => cls.Label?.ToLower().RemoveAccents().Contains(terms[i]) == true
                                                  || cls.Id.ToLower().RemoveAccents().Contains(terms[i]))
                                    .ToList();
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyCollection<object>>(QueryType.Class,
                                        resultClasses.Count == 0 ? null : resultClasses));
                                break;

                            case QueryType.Individual:
                                var searchIndividuals = _ontologyManager.SearchIndividuals(terms[i]);
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyCollection<object>>(QueryType.Individual,
                                        searchIndividuals));
                                break;

                            case QueryType.Relation:
                                var relations = _ontologyManager.GetRelations();
                                var resultRelations = relations
                                    .Where(r => r.Label?.ToLower().RemoveAccents().Contains(terms[i]) == true
                                                || r.Id.ToLower().RemoveAccents().Contains(terms[i]))
                                    .ToList();
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyCollection<object>>(QueryType.Relation,
                                        resultRelations.Count == 0 ? null : resultRelations));
                                break;

                            case QueryType.Attribute:
                                var attributes = _ontologyManager.GetAttributes();
                                var resultAttributes = attributes
                                    .Where(a => a.Label?.ToLower().RemoveAccents().Contains(terms[i]) == true
                                                || a.Id.ToLower().RemoveAccents().Contains(terms[i]))
                                    .ToList();
                                results[terms[i]].Add(
                                    new KeyValuePair<QueryType, IReadOnlyCollection<object>>(QueryType.Attribute,
                                        resultAttributes.Count == 0 ? null : resultAttributes));
                                break;
                        }
                }

                break;
            }

            if (results.Count == 1)
            {
                var result = results.First();
                var hasOnlyOneResult = false;
                Tuple<string, QueryType, object> oneResult = null;
                foreach (var values in result.Value)
                    if (values.Value != null)
                    {
                        if (values.Value.Count > 1 || hasOnlyOneResult)
                        {
                            hasOnlyOneResult = false;
                            break;
                        }

                        hasOnlyOneResult = true;
                        oneResult = new Tuple<string, QueryType, object>(result.Key, values.Key, values.Value.First());
                    }

                if (hasOnlyOneResult)
                    switch (oneResult.Item2)
                    {
                        case QueryType.Class:
                            return RedirectToAction("Class", "Ontology",
                                new {className = ((Class) oneResult.Item3).Id, keywords = oneResult.Item1});
                        case QueryType.Individual:
                            return RedirectToAction("Individual", "Ontology",
                                new {individualName = ((Individual) oneResult.Item3).Id, keywords = oneResult.Item1});
                        case QueryType.Relation:
                            return RedirectToAction("Relation", "Ontology",
                                new {relationName = ((Relation) oneResult.Item3).Id, keywords = oneResult.Item1});
                        case QueryType.Attribute:
                            return RedirectToAction("Attribute", "Ontology",
                                new {attributeName = ((Attribute) oneResult.Item3).Id, keywords = oneResult.Item1});
                    }
            }

            return results.Count == 0 ? View(null) : View(results);
        }
    }
}