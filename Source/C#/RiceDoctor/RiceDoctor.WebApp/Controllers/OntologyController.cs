using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using RiceDoctor.OntologyManager;
using RiceDoctor.SemanticCodeInterpreter;
using RiceDoctor.Shared;
using Attribute = RiceDoctor.OntologyManager.Attribute;

namespace RiceDoctor.WebApp.Controllers
{
    public class OntologyController : Controller
    {
        [NotNull] private readonly IOntologyManager _ontologyManager;
        [NotNull] private readonly ISemanticCodeInterpreter _semanticCodeInterpreter;

        public OntologyController(
            [FromServices] [NotNull] ISemanticCodeInterpreter semanticCodeInterpreter,
            [FromServices] [NotNull] IOntologyManager ontologyManager)
        {
            Check.NotNull(semanticCodeInterpreter, nameof(semanticCodeInterpreter));
            Check.NotNull(ontologyManager, nameof(ontologyManager));

            _semanticCodeInterpreter = semanticCodeInterpreter;
            _ontologyManager = ontologyManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Class(string className, bool showAdvance = false)
        {
            className = string.IsNullOrWhiteSpace(className) ? _ontologyManager.ThingClassId : className.Trim();

            var @class = _ontologyManager.GetClass(className);
            if (@class == null)
                return RedirectToAction("Error", "Home", new {error = $"Class \"{className}\" not found."});

            var directSuperClassesTree = new List<IList<Class>>();
            GetSuperClassesTree(@class.GetDirectSuperClasses()?.ToList(), directSuperClassesTree, 0);

            var classBuilder = new StringBuilder();
            GetClassTree(@class, directSuperClassesTree, classBuilder);
            ViewData["ClassTree"] = "[" + classBuilder + "]";

            var individualBuilder = new StringBuilder();
            GetIndividualTree(@class, individualBuilder, 0);
            ViewData["IndividualTree"] = "[" + individualBuilder + "]";

            ViewData["ShowAdvance"] = showAdvance;

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
                            return _semanticCodeInterpreter.Parse(v);
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
                        ViewData["Article"] = _semanticCodeInterpreter.Parse(pair.Value.First());
                        return View(individual);
                    }
            }

            return RedirectToAction("Individual", new {individualName});
        }

        private void GetSuperClassesTree(
            [CanBeNull] IList<Class> superClasses,
            [NotNull] IList<IList<Class>> superClassesTree,
            int level)
        {
            Check.NotNull(superClassesTree, nameof(superClassesTree));

            if (level > 10) throw new InvalidOperationException(nameof(GetSuperClassesTree));

            if (superClasses != null)
            {
                superClassesTree.Insert(0, superClasses);

                var newSuperSuperClasses = new List<Class>();
                foreach (var superClass in superClasses)
                {
                    if (superClass.GetDirectSuperClasses() == null) continue;
                    foreach (var superSuperClass in superClass.GetDirectSuperClasses())
                        if (!newSuperSuperClasses.Contains(superSuperClass))
                            newSuperSuperClasses.Add(superSuperClass);
                }

                if (newSuperSuperClasses.Count > 0)
                    GetSuperClassesTree(newSuperSuperClasses, superClassesTree, level + 1);
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

            if (@class.Id == _ontologyManager.ThingClassId) GetCurrentClassTree(@class, builder);
            if (superClassesTree.Count == 0) return;

            var superClass = superClassesTree[0].First();
            superClassesTree.RemoveAt(0);

            builder.AppendLine("{");
            builder.AppendLine($"text: '{superClass}',");
            builder.AppendLine("icon: 'glyphicon glyphicon-copyright-mark',");
            builder.AppendLine($"href: '{Url.Action("Class", "Ontology", new {className = superClass.Id})}',");
            builder.AppendLine("state: { expanded: true },");
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
                    if (directSubClass.GetDirectSubClasses() != null) builder.AppendLine("nodes: []");
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
            builder.AppendLine($"href: '{Url.Action("Class", "Ontology", new {className = @class.Id})}',");
            builder.AppendLine("state: { selected: true, expanded: true },");

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
                    if (directSubClasses[i].GetDirectSubClasses() != null) builder.AppendLine("nodes: []");
                    builder.AppendLine("}");
                }

                builder.AppendLine("]");
            }
            builder.AppendLine("},");
        }

        private void GetIndividualTree([NotNull] Class @class, [NotNull] StringBuilder builder, int level)
        {
            Check.NotNull(@class, nameof(@class));
            Check.NotNull(builder, nameof(builder));

            if (level > 10) throw new InvalidOperationException(nameof(GetIndividualTree));

            var directIndividuals = @class.GetDirectIndividuals();
            var subClasses = @class.GetDirectSubClasses();

            if (level != 0)
            {
                builder.AppendLine("{");
                builder.AppendLine($"text: '{@class}',");
                builder.AppendLine("icon: 'glyphicon glyphicon-copyright-mark',");
                builder.AppendLine($"href: '{Url.Action("Class", "Ontology", new {className = @class.Id})}',");
                builder.AppendLine("state: { expanded: true },");
                builder.AppendLine($"tags: [ '{@class.GetAllIndividuals()?.Count ?? 0} thể hiện' ],");
                if (directIndividuals != null || subClasses != null) builder.AppendLine("nodes: [");
            }

            if (directIndividuals != null) GetDirectIndividualTree(directIndividuals, builder, level == 0);

            if (subClasses != null)
                foreach (var subClass in subClasses) GetIndividualTree(subClass, builder, level + 1);

            if (level != 0)
            {
                if (directIndividuals != null || subClasses != null) builder.AppendLine("]");
                builder.AppendLine("},");
            }
        }

        private void GetDirectIndividualTree(
            [NotNull] IReadOnlyCollection<Individual> individuals,
            [NotNull] StringBuilder builder,
            bool bold)
        {
            Check.NotNull(individuals, nameof(individuals));
            Check.NotNull(builder, nameof(builder));

            foreach (var individual in individuals)
            {
                builder.AppendLine("{");
                builder.AppendLine(bold
                    ? $"text: '<strong>{individual}</strong>',"
                    : $"text: '{individual}',");
                builder.AppendLine("icon: 'glyphicon glyphicon-info-sign',");
                builder.AppendLine(
                    $"href: '{Url.Action("Individual", "Ontology", new {individualName = individual.Id})}'");
                builder.AppendLine("},");
            }
        }
    }
}