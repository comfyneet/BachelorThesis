using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public IActionResult Class(string className, bool showAdvance = false)
        {
            className = string.IsNullOrWhiteSpace(className) ? "Thing" : className.Trim();

            var @class = _ontologyManager.GetClass(className);
            if (@class == null)
                return RedirectToAction("Error", "Home", new {error = $"Class \"{className}\" not found."});

            var directSuperClassesTree = new List<IList<Class>>();
            GetDirectSuperClassesTree(@class.GetDirectSuperClasses()?.ToList(), directSuperClassesTree, 0);

            ViewData["DirectSuperClassesTree"] = directSuperClassesTree;
            ViewData["ShowAdvance"] = showAdvance;

            var builder = new StringBuilder();
            GetClassTree(@class, directSuperClassesTree, builder);
            ViewData["ClassTree"] = "[" + builder + "]";

            return View(@class);
        }

        public IActionResult Relation(string relationName)
        {
            Relation relation = null;
            if (!string.IsNullOrWhiteSpace(relationName))
            {
                relationName = relationName.Trim();
                relation = _ontologyManager.GetRelation(relationName);
                if (relation == null)
                    return RedirectToAction("Error", "Home", new {error = $"Relation \"{relationName}\" not found."});
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
                if (attribute == null)
                    return RedirectToAction("Error", "Home", new {error = $"Attribute \"{attributeName}\" not found."});
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
                if (individual == null)
                    return RedirectToAction("Error", "Home",
                        new {error = $"Individual \"{individualName}\" not found."});

                ViewData["AttributeValues"] = individual.GetAttributeValues()?
                    .Select(av => new KeyValuePair<Attribute, IReadOnlyCollection<string>>(
                        av.Key,
                        av.Value.Select(v =>
                        {
                            if (av.Key.Id == "image" || av.Key.Id == "name") return v;
                            return SemanticParser.Parse(v);
                        }).ToList()))
                    .ToDictionary(av => av.Key, av => av.Value);
            }

            if (!string.IsNullOrWhiteSpace(keywords)) ViewData["Keywords"] = keywords.Trim();

            return View(individual);
        }

        public IActionResult Article(string individualName)
        {
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

            if (count > 10) throw new InvalidOperationException(nameof(GetDirectSuperClassesTree));

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

        private void GetClassTree(
            [NotNull] Class @class,
            [NotNull] IList<IList<Class>> superClassesTree,
            [NotNull] StringBuilder builder)
        {
            Check.NotNull(@class, nameof(@class));
            Check.NotNull(superClassesTree, nameof(superClassesTree));
            Check.NotNull(builder, nameof(builder));

            if (@class.Id == "Thing") GetCurrentClassTree(@class, builder);
            if (superClassesTree.Count == 0) return;

            var superClass = superClassesTree[0].First();
            superClassesTree.RemoveAt(0);

            builder.AppendLine("{");
            builder.AppendLine($"text: '{superClass}',");
            builder.AppendLine("icon: 'glyphicon glyphicon-copyright-mark',");
            builder.AppendLine(
                $"href: '{Url.Action("Class", "Ontology", new {className = superClass.Id})}',");
            builder.AppendLine("state: { expanded: true },");
            builder.AppendLine("tags: [");
            var directIndividuals = superClass.GetDirectIndividuals() == null
                ? 0
                : superClass.GetDirectIndividuals().Count;
            var allIndividuals = superClass.GetAllIndividuals() == null
                ? 0
                : superClass.GetAllIndividuals().Count;
            builder.AppendLine($"'direct: {directIndividuals}',");
            builder.AppendLine($"'all: {allIndividuals}' ],");
            builder.AppendLine("nodes: [");

            var directSubClasses = superClass.GetDirectSubClasses();
            foreach (var directSubClass in directSubClasses)
            {
                var nextSuberClass = superClassesTree.Count > 0 ? superClassesTree[0].First() : null;
                if (nextSuberClass != null && directSubClass == nextSuberClass)
                {
                    GetClassTree(@class, superClassesTree, builder);
                }
                else if (directSubClass == @class)
                {
                    GetCurrentClassTree(@class, builder);
                }
                else
                {
                    builder.AppendLine("{");
                    builder.AppendLine($"text: '{directSubClass}',");
                    builder.AppendLine("icon: 'glyphicon glyphicon-copyright-mark',");
                    builder.AppendLine(
                        $"href: '{Url.Action("Class", "Ontology", new {className = directSubClass.Id})}',");
                    builder.AppendLine("tags: [");
                    var directIndividualCount = directSubClass.GetDirectIndividuals() == null
                        ? 0
                        : directSubClass.GetDirectIndividuals().Count;
                    var allIndividualCount = directSubClass.GetAllIndividuals() == null
                        ? 0
                        : directSubClass.GetAllIndividuals().Count;
                    builder.AppendLine($"'direct: {directIndividualCount}',");
                    builder.AppendLine($"'all: {allIndividualCount}' ],");
                    if (directSubClass.GetDirectSubClasses() != null)
                        builder.AppendLine("nodes: []");
                    builder.AppendLine("},");
                }
            }
            builder.Append("]},");
        }

        private void GetCurrentClassTree([NotNull] Class @class, [NotNull] StringBuilder builder)
        {
            Check.NotNull(@class, nameof(@class));
            Check.NotNull(builder, nameof(builder));

            builder.AppendLine("{");
            builder.AppendLine($"text: '{@class}',");
            builder.AppendLine("icon: 'glyphicon glyphicon-copyright-mark',");
            builder.AppendLine(
                $"href: '{Url.Action("Class", "Ontology", new {className = @class.Id})}',");
            builder.AppendLine("state: { selected: true, expanded: true },");
            builder.AppendLine("tags: [");
            var directIndividuals = @class.GetDirectIndividuals() == null
                ? 0
                : @class.GetDirectIndividuals().Count;
            var allIndividuals = @class.GetAllIndividuals() == null
                ? 0
                : @class.GetAllIndividuals().Count;
            builder.AppendLine($"'direct: {directIndividuals}',");
            builder.AppendLine($"'all: {allIndividuals}' ],");

            var directSubClasses = @class.GetDirectSubClasses();
            if (directSubClasses != null)
            {
                builder.AppendLine("nodes: [");

                for (var i = 0; i < directSubClasses.Count; ++i)
                {
                    if (i > 0) builder.Append(',');
                    builder.AppendLine("{");
                    builder.AppendLine($"text: '{directSubClasses[i]}',");
                    builder.AppendLine("icon: 'glyphicon glyphicon-copyright-mark',");
                    builder.AppendLine(
                        $"href: '{Url.Action("Class", "Ontology", new {className = directSubClasses[i].Id})}',");
                    builder.AppendLine("tags: [");
                    var directIndividualCount = directSubClasses[i].GetDirectIndividuals() == null
                        ? 0
                        : directSubClasses[i].GetDirectIndividuals().Count;
                    var allIndividualCount = directSubClasses[i].GetAllIndividuals() == null
                        ? 0
                        : directSubClasses[i].GetAllIndividuals().Count;
                    builder.AppendLine($"'direct: {directIndividualCount}',");
                    builder.AppendLine($"'all: {allIndividualCount}' ],");
                    if (directSubClasses[i].GetDirectSubClasses() != null)
                        builder.AppendLine("nodes: [],");
                    builder.AppendLine("}");
                }

                builder.AppendLine("]");
            }
            builder.AppendLine("},");
        }
    }
}