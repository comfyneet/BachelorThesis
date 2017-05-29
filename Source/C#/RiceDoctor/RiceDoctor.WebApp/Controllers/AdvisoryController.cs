using System;
using System.Collections.Generic;
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
using ResponseType = RiceDoctor.InferenceEngine.ResponseType;

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
            if (string.IsNullOrWhiteSpace(guid)) return NotFound($"Malformed {nameof(guid)}.");
            guid = guid.Trim();

            if (string.IsNullOrWhiteSpace(inputs)) return NotFound($"Malformed {nameof(inputs)}.");
            var inputList = inputs.Trim().Replace("\r\n", "\n").Split('\n');
            var facts = new Fact[inputList.Length];
            for (var i = 0; i < inputList.Length; ++i)
                facts[i] = new IndividualFact(inputList[i].Split('=')[0], inputList[i].Split('=')[1]);

            if (problemId < -1 || problemId >= _ruleManager.Problems.Count)
                return NotFound($"Malformed {nameof(problemId)}.");
            Problem problem;
            if (problemId == -1)
            {
                if (string.IsNullOrWhiteSpace(outputs)) return NotFound("Malformed outputs.");
                var outputList = outputs.Trim().Replace("\r\n", "\n").Split('\n');

                var allTypes = new Dictionary<string, Class>();

                var goalTypes = new List<Class>();
                foreach (var output in outputList)
                {
                    if (!allTypes.TryGetValue(output, out Class goalType))
                    {
                        goalType = _ontologyManager.GetClass(output);
                        if (goalType == null) return NotFound($"Type '{output}' doesn't exist.");
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
                        if (suggestType == null) return NotFound($"Type '{fact.Name}' doesn't exist.");
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

            return View(advisory);
        }

        [HttpPost]
        public IActionResult GuessableFact(string guid, string className, string individualName)
        {
            if (string.IsNullOrWhiteSpace(guid)) return NotFound($"Malformed {nameof(guid)}.");
            guid = guid.Trim();

            if (string.IsNullOrWhiteSpace(className)) return NotFound($"Malformed {nameof(className)}.");
            className = className.Trim();

            if (string.IsNullOrWhiteSpace(individualName)) return NotFound($"Malformed {nameof(individualName)}.");
            individualName = individualName.Trim();

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);

            ViewData["ClassName"] = className;
            ViewData["IndividualName"] = individualName;

            return View("GuessableFact", advisory);
        }

        [HttpPost]
        public IActionResult Infer(string guid, string className = null, string individualName = null,
            int? isGuessable = null)
        {
            if (string.IsNullOrWhiteSpace(guid)) return NotFound($"Malformed {nameof(guid)}.");
            guid = guid.Trim();

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);
            ((Engine) advisory.Engine)._ontologyManager = _ontologyManager;
            ((Engine) advisory.Engine)._ruleManager = _ruleManager;

            if (!string.IsNullOrWhiteSpace(className))
            {
                if (string.IsNullOrWhiteSpace(className)) return NotFound($"Malformed {nameof(className)}.");
                className = className.Trim();

                if (string.IsNullOrWhiteSpace(individualName)) return NotFound($"Malformed {nameof(individualName)}.");
                individualName = individualName.Trim();

                bool? exist = null;
                if (isGuessable == 1) exist = true;
                else if (isGuessable == 0) exist = false;
                else if (isGuessable != -1) return NotFound($"Malformed {nameof(isGuessable)}.");
                ;

                advisory.Engine.HandleGuessableFact(
                    new Tuple<Fact, bool?>(new IndividualFact(className, individualName), exist));
            }

            var response = advisory.Engine.Infer();
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));

            if (response.Type == ResponseType.GuessableFact)
                return GuessableFact(guid, response.GuessableFact.Name,
                    ((IndividualFact) response.GuessableFact).Individual);

            if (response.Type == InferredResults)
            {
                advisory.Results = response.Results;
                HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));
            }

            return View("Infer", advisory);
        }
    }
}