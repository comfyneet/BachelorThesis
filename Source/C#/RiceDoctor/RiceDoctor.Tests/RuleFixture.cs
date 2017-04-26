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

            var logicPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\logic-rules.txt");
            var logicData = File.ReadAllText(logicPath);

            var relationPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\relation-rules.txt");
            var relationData = File.ReadAllText(relationPath);

            RuleManager = new Manager(problemData, logicData, relationData);

            Assert.NotNull(RuleManager.LogicRules);
        }

        public IRuleManager RuleManager { get; }
    }
}