using System.Collections.Generic;

namespace RiceDoctor.Tests
{
    public class InferenceTestOutput
    {
        public IReadOnlyCollection<TestFact> InferredFacts { get; set; }

        public IReadOnlyCollection<TestFact> ExpectedFacts { get; set; }
    }
}