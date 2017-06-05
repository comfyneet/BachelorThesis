using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RiceDoctor.InferenceEngine;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using RiceDoctor.WebApp.Models;
using static RiceDoctor.InferenceEngine.ResponseType;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Request = RiceDoctor.InferenceEngine.Request;
using RequestType = RiceDoctor.InferenceEngine.RequestType;

namespace RiceDoctor.WebApp.Controllers
{
    public class AdvisoryController : Controller
    {
        private readonly IOntologyManager _ontologyManager;
        private readonly IRuleManager _ruleManager;

        private readonly JsonSerializerSettings jsonSettings =
            new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};

        public AdvisoryController(
            [FromServices] [NotNull] IRuleManager ruleManager,
            [FromServices] [NotNull] IOntologyManager ontologyManager)
        {
            Check.NotNull(ruleManager, nameof(ruleManager));
            Check.NotNull(ontologyManager, nameof(ontologyManager));

            _ruleManager = ruleManager;
            _ontologyManager = ontologyManager;
        }

        public IActionResult Index()
        {
            ViewData["Problems"] = _ruleManager.Problems;

            var guid = Guid.NewGuid().ToString();
            var advisory = new Advisory {Guid = guid};
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));


            return View(advisory);
        }

        [HttpPost]
        public IActionResult SelectProblem(string guid, int problemId, string inputs, string outputs, int? totalGoals)
        {
            if (string.IsNullOrWhiteSpace(guid)) return NotFound($"Malformed {nameof(guid)}");
            guid = guid.Trim();

            if (string.IsNullOrWhiteSpace(inputs)) return NotFound($"Malformed {nameof(inputs)}");
            var inputList = inputs.Trim().Replace("\r\n", "\n").Split('\n');
            var facts = new Fact[inputList.Length];
            for (var i = 0; i < inputList.Length; ++i)
                facts[i] = new IndividualFact(inputList[i].Split('=')[0], inputList[i].Split('=')[1]);

            if (problemId < -1 || problemId >= _ruleManager.Problems.Count)
                return NotFound($"Malformed {nameof(problemId)}.");
            Problem problem;
            if (problemId == -1)
            {
                if (string.IsNullOrWhiteSpace(outputs)) return NotFound("Malformed outputs");
                var outputList = outputs.Trim().Replace("\r\n", "\n").Split('\n');

                var allTypes = new Dictionary<string, Class>();

                var goalTypes = new List<Class>();
                foreach (var output in outputList)
                {
                    if (!allTypes.TryGetValue(output, out Class goalType))
                    {
                        goalType = _ontologyManager.GetClass(output);
                        if (goalType == null) return NotFound($"Type '{output}' doesn't exist");
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
                        if (suggestType == null) return NotFound($"Type '{fact.Name}' doesn't exist");
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

            if (totalGoals != null && totalGoals <= 0) return NotFound($"Malformed {nameof(totalGoals)}");

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);
            advisory.Request = new Request(problem, RequestType.IndividualFact, totalGoals);
            advisory.Engine = new Engine(_ruleManager, _ontologyManager, advisory.Request);
            advisory.Engine.AddFactsToKnown(facts);
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));

            //return View(advisory);
            return Infer(guid);
        }

        [HttpPost]
        public IActionResult GuessableFact(string guid, IReadOnlyCollection<Fact> guessableFacts)
        {
            if (string.IsNullOrWhiteSpace(guid)) return NotFound($"Malformed {nameof(guid)}");
            guid = guid.Trim();

            if (guessableFacts == null) return NotFound($"Malformed {nameof(guessableFacts)}");

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);

            ViewData["GuessableFacts"] = guessableFacts.ToList();

            return View("GuessableFact", advisory);
        }

        [HttpPost]
        public IActionResult Infer(string guid, IReadOnlyCollection<GuessableFact> guessableFacts = null)
        {
            if (string.IsNullOrWhiteSpace(guid)) return NotFound($"Malformed {nameof(guid)}");
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
                        return NotFound($"Malformed {nameof(fact.ClassName)}");
                    fact.ClassName = fact.ClassName.Trim();

                    if (string.IsNullOrWhiteSpace(fact.IndividualName))
                        return NotFound($"Malformed {nameof(fact.IndividualName)}");
                    fact.IndividualName = fact.IndividualName.Trim();

                    bool? exist = null;
                    if (fact.IsGuessable == 1) exist = true;
                    else if (fact.IsGuessable == 0) exist = false;
                    else if (fact.IsGuessable != -1) return NotFound($"Malformed {nameof(fact.IsGuessable)}");

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