using JetBrains.Annotations;
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
    }
}