using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
using RiceDoctor.SemanticCode;
using RiceDoctor.Shared;

namespace RiceDoctor.WebApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly IOntologyManager _ontologyManager;

        public SearchController([FromServices] [NotNull] IOntologyManager ontologyManager)
        {
            Check.NotNull(ontologyManager, nameof(ontologyManager));

            _ontologyManager = ontologyManager;
        }

        [HttpPost]
        public IActionResult Index(string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Home");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;

            var tmpSearchIndividuals = _ontologyManager.SearchIndividuals(keywords);
            if (tmpSearchIndividuals == null) return View(null);

            var searchIndividuals =
                new Dictionary<Individual, IReadOnlyDictionary<Attribute, IReadOnlyCollection<string>>>();

            if (tmpSearchIndividuals.Count == 1)
            {
                var searchIndividual = tmpSearchIndividuals.First();
                if (searchIndividual.Value == null) return RedirectToAction("Article", "Ontology", new {individualName = searchIndividual.Key.Id});
            }

            foreach (var searchIndividual in tmpSearchIndividuals)
                if (searchIndividual.Value == null) searchIndividuals[searchIndividual.Key] = null;
                else
                    searchIndividuals[searchIndividual.Key] = searchIndividual.Value
                        .Select(av => new KeyValuePair<Attribute, IReadOnlyCollection<string>>(
                            av.Key,
                            av.Value.Select(SemanticParser.Parse).ToList()))
                        .ToDictionary(av => av.Key, av => av.Value);

            return View(searchIndividuals);
        }
    }
}