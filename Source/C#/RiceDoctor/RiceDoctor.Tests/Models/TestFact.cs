using System;
using RiceDoctor.RuleManager;

namespace RiceDoctor.Tests
{
    public class TestFact
    {
        public TestFactType Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public Fact ToOntologyFact()
        {
            if (Type == TestFactType.ScalarFact)
                return new ScalarFact(Name, Value);

            if (Type == TestFactType.IndividualFact)
                return new IndividualFact(Name, Value);

            throw new NotImplementedException();
        }
    }
}