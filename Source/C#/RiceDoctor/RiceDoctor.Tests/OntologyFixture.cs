using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using Xunit;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class OntologyFixture
    {
        public OntologyFixture()
        {
            OntologyManager = Manager.Instance;
        }

        [NotNull]
        public IOntologyManager OntologyManager { get; }
    }
}