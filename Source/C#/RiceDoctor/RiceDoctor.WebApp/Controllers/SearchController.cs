using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
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
            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Home");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;

            var unaccentKeywords = keywords.ToLower().RemoveAccents();
            var results = new List<Tuple<string, QueryType, IReadOnlyCollection<object>>>();

            foreach (var query in _queryManager.Queries)
            {
                var pattern = query.Node.ToString();
                var regex = new Regex(pattern);
                var match = regex.Match(unaccentKeywords);
                if (match.Success)
                {
                    for (var i = 1; i < match.Groups.Count; ++i)
                    {
                        var value = match.Groups[i].Value;
                        var resultTypes = query.ResultTypes[i - 1];
                        foreach (var resultType in resultTypes)
                            switch (resultType)
                            {
                                case QueryType.Class:
                                    var classes = _ontologyManager.GetSubClasses("Thing", GetAll);
                                    var resultClasses = classes
                                        .Where(cls => cls.Label?.ToLower().RemoveAccents().Contains(value)
                                                      == true ||
                                                      cls.Id.ToLower().RemoveAccents().Contains(value))
                                        .ToList();
                                    if (resultClasses.Count == 1 && match.Groups.Count == 2)
                                        return RedirectToAction("Class", "Ontology",
                                            new {className = resultClasses.First().Id, keywords});
                                    else if (resultClasses.Count > 1)
                                        results.Add(
                                            new Tuple<string, QueryType, IReadOnlyCollection<object>>(value,
                                                QueryType.Class, resultClasses));
                                    break;

                                case QueryType.Individual:
                                    var searchIndividuals = _ontologyManager.SearchIndividuals(value);
                                    if (searchIndividuals != null)
                                    {
                                        if (searchIndividuals.Count == 1 && match.Groups.Count == 2)
                                            return RedirectToAction("Individual", "Ontology",
                                                new {individualName = searchIndividuals.First().Id, keywords});
                                        results.Add(
                                            new Tuple<string, QueryType, IReadOnlyCollection<object>>(value,
                                                QueryType.Individual, searchIndividuals));
                                    }
                                    break;

                                case QueryType.Relation:
                                    var relations = _ontologyManager.GetRelations();
                                    var resultRelations = relations
                                        .Where(r => r.Label?.ToLower().RemoveAccents().Contains(value)
                                                    == true ||
                                                    r.Id.ToLower().RemoveAccents().Contains(value))
                                        .ToList();
                                    if (resultRelations.Count == 1 && match.Groups.Count == 2)
                                        return RedirectToAction("Relation", "Ontology",
                                            new {relationName = resultRelations.First().Id, keywords});
                                    else if (resultRelations.Count > 1)
                                        results.Add(
                                            new Tuple<string, QueryType, IReadOnlyCollection<object>>(value,
                                                QueryType.Relation, resultRelations));
                                    break;

                                case QueryType.Attribute:
                                    var attributes = _ontologyManager.GetAttributes();
                                    var resultAttributes = attributes
                                        .Where(a => a.Label?.ToLower().RemoveAccents().Contains(value)
                                                    == true ||
                                                    a.Id.ToLower().RemoveAccents().Contains(value))
                                        .ToList();
                                    if (resultAttributes.Count == 1 && match.Groups.Count == 2)
                                        return RedirectToAction("Attribute", "Ontology",
                                            new {attributeName = resultAttributes.First().Id, keywords});
                                    else if (resultAttributes.Count > 1)
                                        results.Add(
                                            new Tuple<string, QueryType, IReadOnlyCollection<object>>(value,
                                                QueryType.Attribute, resultAttributes));
                                    break;
                            }
                    }

                    break;
                }
            }

            return results.Count == 0 ? View(null) : View(results);
        }
    }
}