using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
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

        public IActionResult Index(string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Home");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;

            var searchIndividuals = _ontologyManager.SearchIndividuals(keywords);
            if (searchIndividuals == null) return View(null);

            if (searchIndividuals.Count == 1)
            {
                var searchIndividual = searchIndividuals.First();
                return RedirectToAction("Individual", "Ontology", new {individualName = searchIndividual.Id, keywords});
            }

            return View(searchIndividuals);
        }
    }
}