using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RiceDoctor.FuzzyManager;
using RiceDoctor.InferenceEngine;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using RiceDoctor.WebApp.Models;
using static RiceDoctor.InferenceEngine.ResponseType;
using static RiceDoctor.OntologyManager.GetType;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Request = RiceDoctor.InferenceEngine.Request;
using RequestType = RiceDoctor.InferenceEngine.RequestType;

namespace RiceDoctor.WebApp.Controllers
{
    public class AdvisoryController : Controller
    {
        private readonly IFuzzyManager _fuzzyManager;
        private readonly IOntologyManager _ontologyManager;
        private readonly IRuleManager _ruleManager;

        private readonly JsonSerializerSettings jsonSettings =
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};

        public AdvisoryController(
            [FromServices] [NotNull] IRuleManager ruleManager,
            [FromServices] [NotNull] IOntologyManager ontologyManager,
            [FromServices] [NotNull] IFuzzyManager fuzzyManager)
        {
            Check.NotNull(ruleManager, nameof(ruleManager));
            Check.NotNull(ontologyManager, nameof(ontologyManager));
            Check.NotNull(fuzzyManager, nameof(fuzzyManager));

            _ruleManager = ruleManager;
            _ontologyManager = ontologyManager;
            _fuzzyManager = fuzzyManager;
        }

        public IActionResult Index()
        {
            ViewData["Problems"] = _ruleManager.Problems;
            ViewData["Classes"] = _ontologyManager.GetSubClasses("Thing", GetAll);
            ViewData["Individuals"] = _ontologyManager.GetIndividuals();
            ViewData["FuzzyVariables"] = _fuzzyManager.Variables;

            var guid = Guid.NewGuid().ToString();
            var advisory = new Advisory {Guid = guid};
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));


            return View(advisory);
        }

        [HttpPost]
        public IActionResult SelectProblem(string guid, int problemId, List<string> inputs, string fuzzyInputs,
            double? fuzzyValues, List<string> outputs, int? totalGoals)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return RedirectToAction("Error", "Home", new {error = CoreStrings.MalformedArgument(nameof(guid))});
            guid = guid.Trim();

            if (inputs == null || string.IsNullOrWhiteSpace(fuzzyInputs))
                return RedirectToAction("Error", "Home", new {error = CoreStrings.MalformedArgument(nameof(inputs))});
            var facts = new List<Fact>();
            foreach (var input in inputs)
            {
                var individual = _ontologyManager.GetIndividual(input);
                if (individual == null)
                    return RedirectToAction("Error", "Home", new {error = $"Individual \"{input}\" not found."});
                facts.Add(new IndividualFact(individual.GetDirectClass().Id, individual.Id));
            }

            if (!string.IsNullOrWhiteSpace(fuzzyInputs) && fuzzyValues != null)
            {
                fuzzyInputs = fuzzyInputs.Trim();
                var varSymbol = _fuzzyManager.Variables.FirstOrDefault(v => v.Id == fuzzyInputs);
                if (varSymbol == null)
                    return RedirectToAction("Error", "Home", new {error = CoreStrings.MalformedArgument(fuzzyInputs)});

                var newTerms = varSymbol.Stmt.Execute((double) fuzzyValues);
                foreach (var newTerm in newTerms)
                    if (newTerm.Value > 0) facts.Add(new IndividualFact(fuzzyInputs, newTerm.Key));
            }

            if (problemId < -1 || problemId >= _ruleManager.Problems.Count)
                return RedirectToAction("Error", "Home",
                    new {error = CoreStrings.MalformedArgument(nameof(problemId))});
            Problem problem;
            if (problemId == -1)
            {
                if (outputs == null || outputs.Count == 0)
                    return RedirectToAction("Error", "Home",
                        new {error = CoreStrings.MalformedArgument(nameof(outputs))});
                var allTypes = new Dictionary<string, Class>();

                var goalTypes = new List<Class>();
                foreach (var output in outputs)
                {
                    if (!allTypes.TryGetValue(output, out Class goalType))
                    {
                        goalType = _ontologyManager.GetClass(output);
                        if (goalType == null)
                            return RedirectToAction("Error", "Home", new {error = CoreStrings.NonexistentType(output)});
                        allTypes.Add(output, goalType);
                    }
                    goalTypes.Add(goalType);
                }

                var suggestTypes = new List<Class>();
                foreach (var fact in facts)
                {
                    if (!allTypes.TryGetValue(fact.Name, out Class suggestType))
                    {
                        suggestType = _ontologyManager.GetClass(fact.Name);
                        if (suggestType == null)
                            return RedirectToAction("Error", "Home",
                                new {error = CoreStrings.NonexistentType(fact.Name)});
                        allTypes.Add(fact.Name, suggestType);
                    }
                    suggestTypes.Add(suggestType);
                }

                problem = new Problem("General", goalTypes, suggestTypes);
            }
            else
            {
                problem = _ruleManager.Problems[problemId];
            }

            if (totalGoals != null && totalGoals <= 0)
                return RedirectToAction("Error", "Home",
                    new {error = CoreStrings.MalformedArgument(nameof(totalGoals))});

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);
            advisory.Request = new Request(problem, RequestType.IndividualFact, totalGoals);
            advisory.Engine = new Engine(_ruleManager, _ontologyManager, advisory.Request);
            advisory.Engine.AddFactsToKnown(facts.ToArray());
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));

            //return View(advisory);
            return Infer(guid);
        }

        [HttpPost]
        public IActionResult GuessableFact(string guid, IReadOnlyCollection<Fact> guessableFacts)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return RedirectToAction("Error", "Home", new {error = CoreStrings.MalformedArgument(nameof(guid))});
            guid = guid.Trim();

            if (guessableFacts == null)
                return RedirectToAction("Error", "Home",
                    new {error = CoreStrings.MalformedArgument(nameof(guessableFacts))});

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);

            ViewData["GuessableFacts"] = guessableFacts.ToList();

            return View("GuessableFact", advisory);
        }

        [HttpPost]
        public IActionResult Infer(string guid, IReadOnlyCollection<GuessableFact> guessableFacts = null)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return RedirectToAction("Error", "Home", new {error = CoreStrings.MalformedArgument(nameof(guid))});
            guid = guid.Trim();

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);
            ((Engine) advisory.Engine)._ontologyManager = _ontologyManager;
            ((Engine) advisory.Engine)._ruleManager = _ruleManager;

            if (guessableFacts != null)
            {
                var facts = new List<Tuple<Fact, bool?>>();
                foreach (var fact in guessableFacts)
                {
                    if (string.IsNullOrWhiteSpace(fact.ClassName))
                        return RedirectToAction("Error", "Home",
                            new {error = CoreStrings.MalformedArgument(nameof(fact.ClassName))});
                    fact.ClassName = fact.ClassName.Trim();

                    if (string.IsNullOrWhiteSpace(fact.IndividualName))
                        return RedirectToAction("Error", "Home",
                            new {error = CoreStrings.MalformedArgument(nameof(fact.IndividualName))});
                    fact.IndividualName = fact.IndividualName.Trim();

                    bool? exist = null;
                    if (fact.IsGuessable == 1) exist = true;
                    else if (fact.IsGuessable == 0) exist = false;
                    else if (fact.IsGuessable != -1)
                        return RedirectToAction("Error", "Home",
                            new {error = CoreStrings.MalformedArgument(nameof(fact.IsGuessable))});

                    facts.Add(new Tuple<Fact, bool?>(new IndividualFact(fact.ClassName, fact.IndividualName), exist));
                }

                advisory.Engine.HandleGuessableFacts(facts);
            }

            var response = advisory.Engine.Infer();
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));

            if (response.Type == GuessableFacts) return GuessableFact(guid, response.Facts);
            if (response.Type == InferredResults)
            {
                advisory.Results = response.Facts;
                HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));
            }
            else
            {
                var incompleteRules = advisory.Engine.GetIncompleteRules();
                ViewData["IncompleteRules"] = incompleteRules;

                if (incompleteRules.Count > 0)
                {
                    var priority = incompleteRules.First().Item1;
                    ViewData["Priority"] = priority * 100;

                    var incompleteFacts = new List<Fact>();
                    foreach (var rule in incompleteRules.Where(r => r.Item1.Equals3DigitPrecision(priority)))
                        incompleteFacts.AddRange(rule.Item3);
                    ViewData["IncompleteFacts"] = incompleteFacts;
                }
            }

            return View("Infer", advisory);
        }
    }
}