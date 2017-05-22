using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
using RiceDoctor.SemanticCode;
using RiceDoctor.Shared;
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
            if (string.IsNullOrEmpty(className)) className = "Thing";

            var @class = _ontologyManager.GetClass(className);
            if (@class == null) return NotFound($"Class \"{className}\" not found.");

            var directSuperClassesTree = new List<IList<Class>>();
            GetDirectSuperClassesTree(@class.DirectSuperClasses?.ToList(), directSuperClassesTree, 0);

            ViewData["Class"] = @class;
            ViewData["DirectSuperClassesTree"] = directSuperClassesTree;
            ViewData["DirectSubClasses"] = @class.DirectSubClasses;
            ViewData["AllSubClasses"] = @class.AllSubClasses;
            ViewData["DirectSuperClasses"] = @class.DirectSuperClasses;
            ViewData["AllSuperClasses"] = @class.AllSuperClasses;
            ViewData["Attributes"] = @class.Attributes;
            ViewData["DomainRelations"] = @class.DomainRelations;
            ViewData["RangeRelations"] = @class.RangeRelations;
            ViewData["DirectIndividuals"] = @class.DirectIndividuals;
            ViewData["AllIndividuals"] = @class.AllIndividuals;

            return View();
        }

        public IActionResult Relation(string relationName)
        {
            if (!string.IsNullOrEmpty(relationName))
            {
                var relation = _ontologyManager.GetRelation(relationName);
                if (relation == null) return NotFound($"Relation \"{relationName}\" not found.");

                ViewData["Relation"] = relation;
                ViewData["InverseRelation"] = relation.InverseRelation;
                ViewData["DirectDomains"] = relation.DirectDomains;
                ViewData["AllDomains"] = relation.AllDomains;
                ViewData["DirectRanges"] = relation.DirectRanges;
                ViewData["AllRanges"] = relation.AllRanges;
            }

            ViewData["Relations"] = _ontologyManager.GetRelations();

            return View();
        }

        public IActionResult Attribute(string attributeName)
        {
            if (!string.IsNullOrEmpty(attributeName))
            {
                var attribute = _ontologyManager.GetAttribute(attributeName);
                if (attribute == null) return NotFound($"Attribute \"{attributeName}\" not found");

                ViewData["Attribute"] = attribute;
                ViewData["DirectDomains"] = attribute.DirectDomains;
                ViewData["AllDomains"] = attribute.AllDomains;
                ViewData["Range"] = attribute.Range;
            }

            ViewData["Attributes"] = _ontologyManager.GetAttributes();

            return View();
        }

        public IActionResult Individual(string individualName)
        {
            if (!string.IsNullOrEmpty(individualName))
            {
                var individual = _ontologyManager.GetIndividual(individualName);
                if (individual == null) return NotFound($"Individual \"{individualName}\" not found");

                ViewData["Individual"] = individual;
                ViewData["DirectClass"] = individual.DirectClass;
                ViewData["AllClasses"] = individual.AllClasses;
                ViewData["RelationValues"] = individual.RelationValues;
                ViewData["AttributeValues"] = individual.AttributeValues?
                    .Select(av => new KeyValuePair<Attribute, IReadOnlyCollection<string>>(
                        av.Key,
                        av.Value.Select(GetSemanticCode).ToList()))
                    .ToDictionary(av => av.Key, av => av.Value);
            }

            ViewData["Individuals"] = _ontologyManager.GetIndividuals();

            return View();
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
                directSuperClassesTree.Add(directSuperClasses);

                var newDirectSuperClasses = new List<Class>();
                foreach (var directSuperClass in directSuperClasses)
                {
                    var tmpDirectSuperClasses = directSuperClass.DirectSuperClasses;
                    if (tmpDirectSuperClasses != null)
                        foreach (var tmpDirectSuperClass in tmpDirectSuperClasses)
                            if (!newDirectSuperClasses.Contains(tmpDirectSuperClass))
                                newDirectSuperClasses.Add(tmpDirectSuperClass);
                }

                if (newDirectSuperClasses.Count > 0)
                    GetDirectSuperClassesTree(newDirectSuperClasses, directSuperClassesTree, count + 1);
            }
        }

        [NotNull]
        private string GetSemanticCode([NotNull] string source)
        {
            Check.NotNull(source, nameof(source));

            var lexer = new SemanticLexer(source);
            var parser = new SemanticParser(lexer);
            return parser.Parse().ToString();
        }
    }
}