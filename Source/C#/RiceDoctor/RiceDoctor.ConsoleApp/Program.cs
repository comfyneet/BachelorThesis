using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RiceDoctor.InferenceEngine;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using static RiceDoctor.InferenceEngine.ResponseType;
using Manager = RiceDoctor.QueryManager.Manager;
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

            var problemPath = Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Resources\query-rules.txt");
            var problemData = File.ReadAllText(problemPath);
            var x = new Manager(problemData).Queries;
            var x1 = x.First();
            var regex = new Regex(x1.Node.ToString());
            var match = regex.Match("");

            var (ruleManager, ontologyManager) = CreateEngine();

            Console.WriteLine("Cac dang bai toan:");
            for (var i = 0; i < ruleManager.Problems.Count; ++i)
                Console.WriteLine($"[{i}] {ruleManager.Problems[i].Type}");

            Console.Write("Chon bai toan (hoac chon -1 cho bai toan dang tong quat): ");
            var type = int.Parse(Console.ReadLine());
            Problem problem;
            Fact[] facts = null;
            if (type == -1)
            {
                facts = GetInputs();
                problem = GetProblem(facts, ontologyManager);
            }
            else
            {
                problem = ruleManager.Problems[type];
            }

            Console.Write("Chon so su kien muc tieu muon tim trong suy dien tien (hoac chon 0 de tim tat ca): ");
            var totalGoals = int.Parse(Console.ReadLine());

            var request = new Request(problem, RequestType.IndividualFact, totalGoals == 0 ? (int?) null : totalGoals);
            IInferenceEngine engine = new Engine(ruleManager, ontologyManager, request);

            PrintRules(engine);

            if (type != -1) facts = GetInputs();

            engine.AddFactsToKnown(facts);

            var response = engine.Infer();
            while (response.Type == GuessableFacts)
            {
                var guessableFacts = new List<Tuple<Fact, bool?>>();
                foreach (var guessableFact in response.Facts)
                {
                    Console.Write($"Su kien {guessableFact} co ton tai khong (Co:0, Khong:1, Khong biet:2)? ");
                    var existInt = int.Parse(Console.ReadLine());

                    bool? exist = null;
                    if (existInt == 0) exist = true;
                    else if (existInt == 1) exist = false;

                    guessableFacts.Add(new Tuple<Fact, bool?>(guessableFact, exist));
                }

                engine.HandleGuessableFacts(guessableFacts);
                response = engine.Infer();
            }

            Console.Write("Ket qua: ");

            if (response.Type == NoResults)
            {
                Console.WriteLine("Suy dien khong thanh cong");
            }
            else
            {
                Console.WriteLine("Suy dien thanh cong");
                foreach (var resultFact in response.Facts)
                    Console.WriteLine(resultFact);
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

            IRuleManager ruleManager = new RuleManager.Manager(problemData, logicData, relationData);
            IOntologyManager ontologyManager = OntologyManager.Manager.Instance;

            return (ruleManager, ontologyManager);
        }

        private static Fact[] GetInputs()
        {
            Console.Write("Nhap so luong su kien input: ");
            var inputCount = int.Parse(Console.ReadLine());

            Console.WriteLine($"Nhap danh sach su kien ({inputCount}): <Ten>=<Gia_Tri> (VD: Benh=BenhVangLa)");
            var facts = new Fact[inputCount];
            for (var i = 0; i < inputCount; ++i)
            {
                var input = Console.ReadLine().Split('=');
                var fact = new IndividualFact(input[0], input[1]);
                facts[i] = fact;
            }

            return facts;
        }

        private static Problem GetProblem(Fact[] inputs, IOntologyManager ontologyManager)
        {
            Console.Write("Nhap so luong loai su kien muc tieu: ");
            var goalCount = int.Parse(Console.ReadLine());

            var allTypes = new Dictionary<string, Class>();

            var goalTypes = new List<Class>();
            Console.WriteLine($"Nhap danh sach loai su kien ({goalCount}): <Ten> (VD: Benh)");
            for (var i = 0; i < goalCount; ++i)
            {
                var tmpGoalType = Console.ReadLine();

                if (!allTypes.TryGetValue(tmpGoalType, out Class goalType))
                {
                    goalType = ontologyManager.GetClass(tmpGoalType);
                    if (goalType == null) throw new ArgumentException(CoreStrings.NonexistentType(tmpGoalType));
                    allTypes.Add(tmpGoalType, goalType);
                }
                goalTypes.Add(goalType);
            }

            var suggestTypes = new List<Class>();
            foreach (var input in inputs)
            {
                if (!allTypes.TryGetValue(input.Name, out Class suggestType))
                {
                    suggestType = ontologyManager.GetClass(input.Name);
                    if (suggestType == null) throw new ArgumentException(CoreStrings.NonexistentType(input.Name));
                    allTypes.Add(input.Name, suggestType);
                }
                suggestTypes.Add(suggestType);
            }

            var problem = new Problem("General", goalTypes, suggestTypes);

            return problem;
        }

        private static void PrintRules(IInferenceEngine engine)
        {
            Console.WriteLine("Danh sach luat:");
            Console.WriteLine($"Luat quan he uu tien cao: {engine.HighPriorityRelationRules.Count}");
            foreach (var rule in engine.HighPriorityRelationRules)
                Console.WriteLine(rule);

            Console.WriteLine($"Luat tuong minh uu tien cao: {engine.HighPriorityLogicRules.Count}");
            foreach (var rule in engine.HighPriorityLogicRules)
                Console.WriteLine(rule);

            Console.WriteLine($"Luat quan he uu tien trung: {engine.MidPriorityRelationRules.Count}");
            foreach (var rule in engine.MidPriorityRelationRules)
                Console.WriteLine(rule);

            Console.WriteLine($"Luat tuong minh uu tien trung: {engine.MidPriorityLogicRules.Count}");
            foreach (var rule in engine.MidPriorityLogicRules)
                Console.WriteLine(rule);

            Console.WriteLine($"Luat quan he uu tien thap: {engine.LowPriorityRelationRules.Count}");
            foreach (var rule in engine.LowPriorityRelationRules)
                Console.WriteLine(rule);

            Console.WriteLine($"Luat tuong minh uu tien thap: {engine.LowPriorityLogicRules.Count}");
            foreach (var rule in engine.LowPriorityLogicRules)
                Console.WriteLine(rule);
        }
    }
}