using System;
using System.Collections.Generic;
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

            Console.Write("Nhap so su kien input: ");
            var inputCount = int.Parse(Console.ReadLine());

            Console.WriteLine("Nhap danh sach su kien: <Ten>=<Gia_Tri> (VD: Benh=BenhVangLa)");
            var facts = new List<Fact>();
            for (var i = 0; i < inputCount; ++i)
            {
                var input = Console.ReadLine().Split('=');
                var fact = new IndividualFact(input[0], input[1]);
                facts.Add(fact);
            }

            var request = new Request(problem, RequestType.IndividualFact, facts);
            IInferenceEngine engine = new Engine(ruleManager, ontologyManager, request);
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
    }
}