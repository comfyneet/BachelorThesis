using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
using RiceDoctor.SemanticCode;
using RiceDoctor.Shared;
using static RiceDoctor.OntologyManager.GetType;
using Attribute = RiceDoctor.OntologyManager.Attribute;

namespace RiceDoctor.WebApp.Controllers
{
    public class OntologyController : Controller
    {
        private readonly IOntologyManager _ontologyManager;

        public OntologyController([FromServices] [NotNull] IOntologyManager ontologyManager)
        {
            Check.NotNull(ontologyManager, nameof(ontologyManager));

            _ontologyManager = ontologyManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Class(string className)
        {
            className = string.IsNullOrWhiteSpace(className) ? "Thing" : className.Trim();

            var @class = _ontologyManager.GetClass(className);
            if (@class == null) return NotFound($"Class \"{className}\" not found");

            var directSuperClassesTree = new List<IList<Class>>();
            GetDirectSuperClassesTree(@class.GetDirectSuperClasses()?.ToList(), directSuperClassesTree, 0);

            ViewData["DirectSuperClassesTree"] = directSuperClassesTree;

            return View(@class);
        }

        public IActionResult Relation(string relationName)
        {
            Relation relation = null;
            if (!string.IsNullOrWhiteSpace(relationName))
            {
                relationName = relationName.Trim();
                relation = _ontologyManager.GetRelation(relationName);
                if (relation == null) return NotFound($"Relation \"{relationName}\" not found");
            }

            ViewData["Relations"] = _ontologyManager.GetRelations();

            return View(relation);
        }

        public IActionResult Attribute(string attributeName)
        {
            Attribute attribute = null;
            if (!string.IsNullOrWhiteSpace(attributeName))
            {
                attributeName = attributeName.Trim();
                attribute = _ontologyManager.GetAttribute(attributeName);
                if (attribute == null) return NotFound($"Attribute \"{attributeName}\" not found");
            }

            ViewData["Attributes"] = _ontologyManager.GetAttributes();

            return View(attribute);
        }

        public IActionResult Individual(string individualName, string keywords = null)
        {
            Individual individual = null;
            if (!string.IsNullOrWhiteSpace(individualName))
            {
                individualName = individualName.Trim();
                individual = _ontologyManager.GetIndividual(individualName);
                if (individual == null) return NotFound($"Individual \"{individualName}\" not found");

                ViewData["AttributeValues"] = individual.GetAttributeValues()?
                    .Select(av => new KeyValuePair<Attribute, IReadOnlyCollection<string>>(
                        av.Key,
                        av.Value.Select(SemanticParser.Parse).ToList()))
                    .ToDictionary(av => av.Key, av => av.Value);
            }

            if (!string.IsNullOrWhiteSpace(keywords)) ViewData["Keywords"] = keywords.Trim();

            return View(individual);
        }

        public IActionResult Individuals()
        {
            var subClasses = _ontologyManager.GetSubClasses("Thing", GetDirect);

            return View(subClasses);
        }

        public IActionResult Article(string individualName, string keywords = null)
        {
            if (!string.IsNullOrWhiteSpace(keywords)) ViewData["Keywords"] = keywords.Trim();

            if (!string.IsNullOrWhiteSpace(individualName))
            {
                individualName = individualName.Trim();
                var individual = _ontologyManager.GetIndividual(individualName);
                var attributeValues = individual?.GetAttributeValues();
                if (attributeValues != null)
                    foreach (var pair in attributeValues)
                    {
                        if (pair.Key.Id != "article") continue;
                        ViewData["Article"] = SemanticParser.Parse(pair.Value.First());
                        return View(individual);
                    }
            }

            return RedirectToAction("Individual", new {individualName});
        }

        private void GetDirectSuperClassesTree(
            [CanBeNull] IList<Class> directSuperClasses,
            [NotNull] IList<IList<Class>> directSuperClassesTree,
            int count)
        {
            Check.NotNull(directSuperClassesTree, nameof(directSuperClassesTree));

            if (count > 10)
                throw new InvalidOperationException(nameof(GetDirectSuperClassesTree));

            if (directSuperClasses != null)
            {
                directSuperClassesTree.Insert(0, directSuperClasses);

                var newDirectSuperClasses = new List<Class>();
                foreach (var directSuperClass in directSuperClasses)
                {
                    var tmpDirectSuperClasses = directSuperClass.GetDirectSuperClasses();
                    if (tmpDirectSuperClasses != null)
                        foreach (var tmpDirectSuperClass in tmpDirectSuperClasses)
                            if (!newDirectSuperClasses.Contains(tmpDirectSuperClass))
                                newDirectSuperClasses.Add(tmpDirectSuperClass);
                }

                if (newDirectSuperClasses.Count > 0)
                    GetDirectSuperClassesTree(newDirectSuperClasses, directSuperClassesTree, count + 1);
            }
        }
    }
}