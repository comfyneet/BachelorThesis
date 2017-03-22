using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.InferenceEngine;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using Xunit;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class InferenceEngineTests : IClassFixture<RuleFixture>, IClassFixture<OntologyFixture>
    {
        [NotNull] private readonly IOntologyManager _ontologyManager;
        [NotNull] private readonly IRuleManager _ruleManager;

        public InferenceEngineTests([NotNull] RuleFixture ruleFixture, [NotNull] OntologyFixture ontologyFixture)
        {
            Check.NotNull(ruleFixture, nameof(ruleFixture));
            Check.NotNull(ontologyFixture, nameof(ontologyFixture));

            _ruleManager = ruleFixture.RuleManager;
            _ontologyManager = ontologyFixture.OntologyManager;
        }

        [NotNull]
        public static IReadOnlyCollection<Fact> MockInputs
        {
            get
            {
                var inputs = new List<Fact>
                {
                    new ScalarFact("Lá", "Vàng"),
                    new ScalarFact("Thân", "Ún")
                };

                return inputs.AsReadOnly();
            }
        }

        [NotNull]
        public IInferenceEngine LoadEngine([NotNull] IRuleManager ruleManager,
            [NotNull] IOntologyManager ontologyManager)
        {
            Check.NotNull(ruleManager, nameof(ruleManager));
            Check.NotNull(ontologyManager, nameof(ontologyManager));

            return new Engine(ruleManager, ontologyManager);
        }

        [Theory]
        [InlineData("Benh")]
        public void InferTrue([NotNull] string className)
        {
            Check.NotEmpty(className, nameof(className));

            var engine = LoadEngine(_ruleManager, _ontologyManager);
            engine.AddFactsToKnown(MockInputs);

            var result = engine.Infer(className);
            Assert.True(result);
        }
    }
}