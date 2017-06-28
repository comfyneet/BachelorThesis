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
            var logicPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\logic-rules.txt");
            var logicData = File.ReadAllText(logicPath);
            var relationPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\relation-rules.txt");
            var relationData = File.ReadAllText(relationPath);
            var ruleManager = new RuleManager.Manager(problemData, logicData, relationData);

            var ontologyManager = OntologyManager.Manager.Instance;

            return (queryManager, fuzzyManager, ruleManager, ontologyManager);
        }
    }
}