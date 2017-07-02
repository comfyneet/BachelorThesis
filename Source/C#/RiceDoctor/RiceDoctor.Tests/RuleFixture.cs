using System;
using System.IO;
using RiceDoctor.RuleManager;
using Xunit;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class RuleFixture
    {
        public RuleFixture()
        {
            var problemPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\problem-types.json");
            var problemData = File.ReadAllText(problemPath);

            var rulePath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\inference-rules.txt");
            var ruleData = File.ReadAllText(rulePath);

            RuleManager = new Manager(problemData, ruleData);

            Assert.NotNull(RuleManager.Rules);
        }

        public IRuleManager RuleManager { get; }
    }
}