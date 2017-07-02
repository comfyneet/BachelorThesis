using System;
using System.IO;
using RiceDoctor.FuzzyManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.QueryManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using Manager = RiceDoctor.QueryManager.Manager;

namespace RiceDoctor.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            Logger.OnLog += logger.Log;

            var (queryManager, fuzzyManager, ruleManager, ontologyManager) = CreateEngine();

            Console.ReadKey();
        }

        private static (IQueryManager, IFuzzyManager, IRuleManager, IOntologyManager) CreateEngine()
        {
            var queryPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\query-rules.txt");
            var queryData = File.ReadAllText(queryPath);
            var queryManager = new Manager(queryData);

            var fuzzyPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\fuzzy-model.txt");
            var fuzzyData = File.ReadAllText(fuzzyPath);
            var fuzzyManager = new FuzzyManager.Manager(fuzzyData);

            var problemPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\problem-types.json");
            var problemData = File.ReadAllText(problemPath);
            var rulePath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\inference-rules.txt");
            var ruleData = File.ReadAllText(rulePath);
            var ruleManager = new RuleManager.Manager(problemData, ruleData);

            var ontologyManager = OntologyManager.Manager.Instance;

            return (queryManager, fuzzyManager, ruleManager, ontologyManager);
        }
    }
}