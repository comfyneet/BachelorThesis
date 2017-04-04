using System.Collections.Generic;

namespace RiceDoctor.Tests
{
    public class InferenceTestInput
    {
        public TestRequest Request { get; set; }

        public IReadOnlyCollection<TestFact> Facts { get; set; }

        public IReadOnlyCollection<string> RelationRules { get; set; }

        public List<string> LogicRules { get; set; }
    }
}