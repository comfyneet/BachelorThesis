using JetBrains.Annotations;
using RiceDoctor.SemanticCodeInterpreter;
using Xunit;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class SemanticCodeFixture
    {
        public SemanticCodeFixture()
        {
            SemanticCodeInterpreter = new Interpreter();
        }

        [NotNull]
        public ISemanticCodeInterpreter SemanticCodeInterpreter { get; }
    }
}