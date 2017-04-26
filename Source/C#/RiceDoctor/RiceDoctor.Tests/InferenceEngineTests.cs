using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RiceDoctor.OntologyManager;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using Xunit;
using IE = RiceDoctor.InferenceEngine;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class InferenceEngineTests : IClassFixture<RuleFixture>, IClassFixture<OntologyFixture>
    {
        public InferenceEngineTests([NotNull] RuleFixture ruleFixture, [NotNull] OntologyFixture ontologyFixture)
        {
            Check.NotNull(ruleFixture, nameof(ruleFixture));
            Check.NotNull(ontologyFixture, nameof(ontologyFixture));

            _ruleManager = ruleFixture.RuleManager;
            _ontologyManager = ontologyFixture.OntologyManager;
        }

        [NotNull] private readonly IOntologyManager _ontologyManager;
        [NotNull] private readonly IRuleManager _ruleManager;

        //public static IEnumerable<object[]> MockTrueData
        //{
        //    get
        //    {
        //        var knownFacts = new List<Fact>
        //        {
        //            new ScalarFact("Lá", "Vàng"),
        //            new ScalarFact("Thân", "Ún")
        //        };

        //        yield return new object[]
        //        {
        //            new IE.Request("Benh", IE.RequestType.IndividualFact, knownFacts),
        //            new List<Fact> {new IndividualFact("Benh", "BenhVangLa")}
        //        };

        //        yield return new object[]
        //        {
        //            new IE.Request("Đầu lá", IE.RequestType.ScalarFact, knownFacts),
        //            new List<Fact> {new IndividualFact("HinhDangLa", "DauLaChay")}
        //        };
        //    }
        //}

        //public static IEnumerable<object[]> MockFalseData
        //{
        //    get
        //    {
        //        var knownFacts = new List<Fact> {new ScalarFact("Lá", "Vàng")};

        //        yield return new object[]
        //        {
        //            new IE.Request("Benh", IE.RequestType.IndividualFact, knownFacts),
        //            new List<Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
        //            {
        //                new Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>(0.5,
        //                    new List<Fact> {new ScalarFact("Thân", "Ún")},
        //                    new List<Fact> {new IndividualFact("Benh", "BenhVangLa")})
        //            }
        //        };

        //        yield return new object[]
        //        {
        //            new IE.Request("FakeFact", IE.RequestType.IndividualFact),
        //            null
        //        };
        //    }
        //}

        //public static IEnumerable<object[]> MockFalseThenTrueData
        //{
        //    get
        //    {
        //        var knownFacts = new List<Fact> {new ScalarFact("Lá", "Vàng")};

        //        yield return new object[]
        //        {
        //            new IE.Request("Benh", IE.RequestType.IndividualFact, knownFacts),
        //            new List<Fact> {new ScalarFact("Thân", "Ún")},
        //            new List<Fact> {new IndividualFact("Benh", "BenhVangLa")}
        //        };

        //        yield return new object[]
        //        {
        //            new IE.Request("HienTuongBenh", IE.RequestType.IndividualFact, knownFacts),
        //            new List<Fact> {new ScalarFact("Thân", "Ún")},
        //            new List<Fact> {new IndividualFact("HienTuongBenh", "HienTuongThieuDam")}
        //        };
        //    }
        //}

        [NotNull]
        public IE.IInferenceEngine CreateEngine()
        {
            return new IE.Engine(_ruleManager, _ontologyManager);
        }

        [Fact]
        public void InferTrue()
        {
            var mockData = new List<ValueTuple<IE.Request, IList<Fact>>>
            {
                (new IE.Request(_ruleManager.Problems.First(p => p.Type == "TrieuChung -> TacNhanGayBenh"),
                    IE.RequestType.IndividualFact, new List<Fact>
                    {
                        new IndividualFact("MauLa", "Test_LaVang"),
                        new IndividualFact("HinhDangThan", "Test_ThanLun")
                    }),
                new List<Fact> {new IndividualFact("Benh", "Test_BenhVangLa")}),

                (new IE.Request(_ruleManager.Problems.First(p => p.Type == "TrieuChung -> TacNhanGayBenh"),
                    IE.RequestType.IndividualFact, new List<Fact>
                    {
                        new IndividualFact("MauLa", "Test_LaVang"),
                        new IndividualFact("HinhDangThan", "Test_ThanUn")
                    }),
                new List<Fact> {new IndividualFact("Benh", "Test_BenhVangLa")})
            };

            foreach (var data in mockData)
            {
                var engine = CreateEngine();
                var actualResultFacts = engine.Infer(data.Item1);
                Assert.True(data.Item2.ScrambledEqual(actualResultFacts));
            }
        }
        //        expectedIncompleteFacts)
        //    [CanBeNull] IReadOnlyList<Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
        //public void InferFalse([NotNull] IE.Request request,
        //[MemberData(nameof(MockFalseData))]

        //[Theory]
        //{
        //    Check.NotNull(request, nameof(request));

        //    var engine = CreateEngine();

        //    var resultFacts = engine.Infer(request);
        //    Assert.Null(resultFacts);

        //    if (expectedIncompleteFacts == null) return;

        //    var actualIncompleteFacts = engine.GetIncompleteFacts(request).ToList();

        //    Assert.Equal(expectedIncompleteFacts.Count, actualIncompleteFacts.Count);
        //    for (var i = 0; i < expectedIncompleteFacts.Count; ++i)
        //    {
        //        Assert.Equal(expectedIncompleteFacts[i].Item1, actualIncompleteFacts[i].Item1, 15);
        //        Assert.True(expectedIncompleteFacts[i].Item2.ScrambledEqual(actualIncompleteFacts[i].Item2));
        //        Assert.True(expectedIncompleteFacts[i].Item3.ScrambledEqual(actualIncompleteFacts[i].Item3));
        //    }
        //}

        //[Theory]
        //[MemberData(nameof(MockFalseThenTrueData))]
        //public void InferFalseThenTrue([NotNull] IE.Request request,
        //    [NotNull] IReadOnlyCollection<Fact> complementFacts,
        //    [NotNull] IReadOnlyCollection<Fact> expectedResultFacts)
        //{
        //    Check.NotNull(request, nameof(request));
        //    Check.NotEmpty(complementFacts, nameof(complementFacts));
        //    Check.NotEmpty(expectedResultFacts, nameof(expectedResultFacts));

        //    var engine = CreateEngine();

        //    var actualResultFacts = engine.Infer(request);
        //    Assert.Null(actualResultFacts);

        //    engine.AddFactsToKnown(complementFacts);

        //    actualResultFacts = engine.Infer(request);
        //    Assert.True(expectedResultFacts.ScrambledEqual(actualResultFacts));
        //}

        //[Fact]
        //public void RunCompleteInferenceTestCases()
        //{
        //    var path = Path.Combine(AppContext.BaseDirectory,
        //        @"..\..\..\..\Resources\Tests\CompleteInferenceTests");
        //    var data = Directory.GetFiles(path, "*.in.json")
        //        .Select(file => new Tuple<string, string>(File.ReadAllText(file),
        //            File.ReadAllText(Path.Combine(path, file.Substring(0, file.IndexOf(".in.json")) + ".out.json"))))
        //        .Select(text => new Tuple<InferenceTestInput, InferenceTestOutput>(
        //            JsonConvert.Deserialize<InferenceTestInput>(text.Item1),
        //            JsonConvert.Deserialize<InferenceTestOutput>(text.Item2)));

        //    foreach (var d in data)
        //    {
        //        var engine = LCreateEngine();

        //        var request = new IE.Request(d.Item1.Request.Name, d.Item1.Request.Type.ToRequestType(),
        //            d.Item1.Facts.Select(f => f.ToOntologyFact()).ToList());

        //        var expectedFacts = d.Item2.ExpectedFacts.Select(f => f.ToOntologyFact());
        //        var actualResultFacts = engine.Infer(request);
        //        Assert.True(expectedFacts.ScrambledEqual(actualResultFacts));
        //    }
        //}
    }
}