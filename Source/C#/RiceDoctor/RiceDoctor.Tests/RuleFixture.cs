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
            var logicPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\logic-rules.txt");
            var logicData = File.ReadAllText(logicPath);

            RuleManager = new Manager(logicData);

            Assert.NotNull(RuleManager.LogicRules);
        }

        public IRuleManager RuleManager { get; }
    }
}