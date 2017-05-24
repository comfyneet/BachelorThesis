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
using ResponseType = RiceDoctor.InferenceEngine.ResponseType;

namespace RiceDoctor.WebApp.Controllers
{
    public class AdvisoryController : Controller
    {
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        private readonly IOntologyManager _ontologyManager;
        private readonly IRuleManager _ruleManager;

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
        public IActionResult SelectProblem(string guid, int problemId, string outputs, string inputs, int totalGoals)
        {
            var inputList = inputs.Contains(',') ? inputs.Split(',') : new[] {inputs};
            var facts = new Fact[inputList.Length];
            for (var i = 0; i < inputList.Length; ++i)
                facts[i] = new IndividualFact(inputList[i].Split('=')[0], inputList[i].Split('=')[1]);

            Problem problem;
            if (problemId == -1)
            {
                var outputList = outputs.Contains(',') ? outputs.Split(',') : new[] {outputs};

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

            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);
            advisory.Request = new Request(problem, RequestType.IndividualFact,
                totalGoals == 0 ? (int?) null : totalGoals);
            advisory.Engine = new Engine(_ruleManager, _ontologyManager, advisory.Request);
            advisory.Engine.AddFactsToKnown(facts);
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));

            return View(advisory);
        }

        public IActionResult GuessableFact(string guid, string className, string individualName)
        {
            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);

            ViewData["ClassName"] = className;
            ViewData["IndividualName"] = individualName;

            return View(advisory);
        }

        [HttpPost]
        public IActionResult GuessableFact(string guid, string className, string individualName, int isGuessable)
        {
            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);

            bool? exist = null;
            if (isGuessable == 1) exist = true;
            else if (isGuessable == 0) exist = false;

            advisory.Engine.HandleGuessableFact(
                new Tuple<Fact, bool?>(new IndividualFact(className, individualName), exist));
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));

            return RedirectToAction("Infer", new {guid});
        }

        [HttpPost]
        public IActionResult Infer(string guid)
        {
            var advisory = JsonConvert.DeserializeObject<Advisory>(HttpContext.Session.GetString(guid), jsonSettings);

            var response = advisory.Engine.Infer();
            HttpContext.Session.SetString(guid, JsonConvert.SerializeObject(advisory, jsonSettings));

            if (response.Type == ResponseType.GuessableFact)
                return RedirectToAction("GuessableFact", new
                {
                    guid,
                    className = response.GuessableFact.Name,
                    individualName = ((IndividualFact) response.GuessableFact).Individual
                });

            if (response.Type == NoResults)
            {
                ViewData["Result"] = false;
            }
            else
            {
                ViewData["Result"] = true;
                advisory.Results = response.Results;
            }

            return View(advisory);
        }
    }
}