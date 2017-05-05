using System;
using System.IO;
using RiceDoctor.InferenceEngine;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using Manager = RiceDoctor.RuleManager.Manager;
using Request = RiceDoctor.InferenceEngine.Request;
using RequestType = RiceDoctor.InferenceEngine.RequestType;

namespace RiceDoctor.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            Logger.OnLog += logger.Log;

            var (ruleManager, ontologyManager) = CreateEngine();

            Console.WriteLine("Cac dang bai toan:");
            for (var i = 0; i < ruleManager.Problems.Count; ++i)
                Console.WriteLine($"[{i}] {ruleManager.Problems[i].Type}");

            Console.Write("Chon bai toan: ");
            var type = int.Parse(Console.ReadLine());
            var problem = ruleManager.Problems[type];

            var request = new Request(problem, RequestType.IndividualFact, null);
            IInferenceEngine engine = new Engine(ruleManager, ontologyManager, request);

            PrintRules(engine);

            Console.Write("Nhap so su kien input: ");
            var inputCount = int.Parse(Console.ReadLine());

            Console.WriteLine("Nhap danh sach su kien: <Ten>=<Gia_Tri> (VD: Benh=BenhVangLa)");
            var facts = new Fact[inputCount];
            for (var i = 0; i < inputCount; ++i)
            {
                var input = Console.ReadLine().Split('=');
                var fact = new IndividualFact(input[0], input[1]);
                facts[i] = fact;
            }

            engine.AddFactsToKnown(facts);

            var resultFacts = engine.Infer();

            Console.Write("Ket qua: ");
            if (resultFacts == null)
            {
                Console.WriteLine("Suy dien tien va lui khong thanh cong");
            }
            else
            {
                Console.WriteLine("Suy dien thanh cong");
                foreach (var resultFact in resultFacts)
                {
                    var individual = (IndividualFact) resultFact;
                    Console.WriteLine($"{individual.Name}={individual.Individual}");
                }
            }

            Console.ReadKey();
        }

        private static (IRuleManager, IOntologyManager) CreateEngine()
        {
            var problemPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\problem-types.json");
            var problemData = File.ReadAllText(problemPath);

            var logicPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\logic-rules.txt");
            var logicData = File.ReadAllText(logicPath);

            var relationPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\relation-rules.txt");
            var relationData = File.ReadAllText(relationPath);

            IRuleManager ruleManager = new Manager(problemData, logicData, relationData);
            IOntologyManager ontologyManager = OntologyManager.Manager.Instance;

            return (ruleManager, ontologyManager);
        }

        private static void PrintRules(IInferenceEngine engine)
        {
            Console.WriteLine("Danh sach luat:");
            Console.WriteLine($"Luat quan he uu tien cao: {engine.HighPriorityRelationRules.Count}");
            foreach (var relationRule in engine.HighPriorityRelationRules)
                Console.WriteLine(relationRule);

            Console.WriteLine($"Luat tuong minh uu tien cao: {engine.HighPriorityLogicRules.Count}");
            foreach (var logicRule in engine.HighPriorityLogicRules)
                Console.WriteLine(logicRule);

            Console.WriteLine($"Luat quan he uu tien thap: {engine.LowPriorityRelationRules.Count}");
            foreach (var relationRule in engine.LowPriorityRelationRules)
                Console.WriteLine(relationRule);

            Console.WriteLine($"Luat tuong minh uu tien thap: {engine.LowPriorityLogicRules.Count}");
            foreach (var logicRule in engine.LowPriorityLogicRules)
                Console.WriteLine(logicRule);
        }
    }
}