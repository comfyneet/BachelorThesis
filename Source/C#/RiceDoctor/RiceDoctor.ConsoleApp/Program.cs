using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RiceDoctor.InferenceEngine;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            Logger.OnLog += logger.Log;

            var (engine, problems) = CreateEngine();

            Console.WriteLine("Cac dang bai toan:");
            for (var i = 0; i < problems.Count; ++i)
                Console.WriteLine($"[{i}] {problems[i].Type}");

            Console.Write("Chon bai toan: ");
            var type = int.Parse(Console.ReadLine());
            var problem = problems[type];

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
            var resultFacts = engine.Infer(request);

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

        private static (IInferenceEngine, IReadOnlyList<Problem>) CreateEngine()
        {
            var problemPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\problem-types.json");
            var problemData = File.ReadAllText(problemPath);

            var logicPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\logic-rules.txt");
            var logicData = File.ReadAllText(logicPath);

            var relationPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\relation-rules.txt");
            var relationData = File.ReadAllText(relationPath);

            var ruleManager = new Manager(problemData, logicData, relationData);
            var ontologyManager = OntologyManager.Manager.Instance;

            return (new Engine(ruleManager, ontologyManager), ruleManager.Problems.ToList());
        }
    }
}