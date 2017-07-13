using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.QueryAnalysis;
using RiceDoctor.RetrievalAnalysis;
using RiceDoctor.Shared;
using RiceDoctor.WebApp.Models;

namespace RiceDoctor.WebApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly IOntologyManager _ontologyManager;
        private readonly IQueryAnalyzer _queryAnalyzer;
        private readonly IRetrievalAnalyzer _retrievalAnalyzer;

        public SearchController(
            [FromServices] [NotNull] IOntologyManager ontologyManager,
            [FromServices] [NotNull] IRetrievalAnalyzer retrievalAnalyzer,
            [FromServices] [NotNull] IQueryAnalyzer queryAnalyzer)
        {
            Check.NotNull(ontologyManager, nameof(ontologyManager));
            Check.NotNull(retrievalAnalyzer, nameof(retrievalAnalyzer));
            Check.NotNull(queryAnalyzer, nameof(queryAnalyzer));

            _ontologyManager = ontologyManager;
            _retrievalAnalyzer = retrievalAnalyzer;
            _queryAnalyzer = queryAnalyzer;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Search(SearchInfor searchInfor)
        {
            if (ModelState.IsValid)
                return searchInfor.IsDocumentSearch
                    ? SearchArticles(searchInfor.Keywords)
                    : SearchOntology(searchInfor.Keywords);

            return View("Index");
        }

        private IActionResult SearchOntology(string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Search");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;
            ViewData["SearchOntology"] = true;

            ViewData["Results"] = _retrievalAnalyzer.FindOntologyEntity(keywords);

            return View("Index");
        }

        private IActionResult SearchArticles(string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords)) return RedirectToAction("Index", "Search");
            keywords = keywords.Trim();

            ViewData["Keywords"] = keywords;
            ViewData["SearchArticles"] = true;

            IReadOnlyCollection<KeyValuePair<Article, double>> results = null;

            foreach (var query in _queryAnalyzer.Queries)
            {
                var terms = query.Match(keywords);
                if (terms == null) continue;

                results = _retrievalAnalyzer.AnalyzeRelevanceRank(terms);

                break;
            }

            ViewData["Results"] = results;

            return View("Index");
        }
    }
}