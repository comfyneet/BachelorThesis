using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;
using Xunit;
using static RiceDoctor.OntologyManager.GetType;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class OntologyManagerTests : IClassFixture<OntologyFixture>
    {
        public OntologyManagerTests([NotNull] OntologyFixture fixture)
        {
            Check.NotNull(fixture.OntologyManager, nameof(fixture), nameof(fixture.OntologyManager));

            _manager = fixture.OntologyManager;
        }

        [NotNull] private readonly IOntologyManager _manager;

        [Theory]
        [InlineData("PhanHuuCo")]
        public void LoadClass([NotNull] string className)
        {
            Check.NotEmpty(className, nameof(className));

            var @class = _manager.GetClass(className);

            var directSuperClasses = _manager.GetSuperClasses(className, GetDirect);
            var allSuperClasses = _manager.GetSuperClasses(className, GetAll);

            var directSubClasses = _manager.GetSubClasses(className, GetDirect);
            var allSubClasses = _manager.GetSubClasses(className, GetAll);

            var domainRelations = _manager.GetDomainRelations(className);
            var rangeRelations = _manager.GetRangeRelations(className);

            var attributes = _manager.GetClassAttributes(className);

            var directIndividuals = _manager.GetClassIndividuals(className, GetDirect);
            var allIndividuals = _manager.GetClassIndividuals(className, GetAll);
        }

        [Theory]
        [InlineData("truBenh")]
        public void LoadRelation([NotNull] string relationName)
        {
            Check.NotEmpty(relationName, nameof(relationName));

            var relation = _manager.GetRelation(relationName);

            var inverseRelation = _manager.GetInverseRelation(relationName);

            var directDomains = _manager.GetRelationDomains(relationName, GetDirect);
            var allDomains = _manager.GetRelationDomains(relationName, GetAll);

            var directRanges = _manager.GetRelationRanges(relationName, GetDirect);
            var allRanges = _manager.GetRelationRanges(relationName, GetAll);
        }

        [Theory]
        [InlineData("moTa")]
        public void LoadAttribute([NotNull] string attributeName)
        {
            Check.NotEmpty(attributeName, nameof(attributeName));

            var attribute = _manager.GetAttribute(attributeName);

            var directDomains = _manager.GetAttributeDomains(attributeName, GetDirect);
            var allDomains = _manager.GetAttributeDomains(attributeName, GetAll);
        }

        [Theory]
        [InlineData("PhanXanh")]
        public void LoadIndividual([NotNull] string individualName)
        {
            Check.NotEmpty(individualName, nameof(individualName));

            var individual = _manager.GetIndividual(individualName);

            var directClass = _manager.GetIndividualClass(individualName);
            var allClasses = _manager.GetIndividualClasses(individualName);

            var relationValues = _manager.GetRelationValues(individualName);
            var attributeValues = _manager.GetAttributeValues(individualName);
        }

        [Fact]
        public void LoadOntology()
        {
            var classes = _manager.GetClass("Thing");
            Assert.NotNull(classes);

            var relations = _manager.GetRelations();
            Assert.NotNull(relations);

            var attributes = _manager.GetAttributes();
            Assert.NotNull(attributes);

            var individuals = _manager.GetIndividuals();
            Assert.NotNull(individuals);
        }
    }
}