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
        [NotNull] private readonly IOntologyManager _ontologyManager;
        [NotNull] private readonly IRuleManager _ruleManager;

        public InferenceEngineTests([NotNull] RuleFixture ruleFixture, [NotNull] OntologyFixture ontologyFixture)
        {
            Check.NotNull(ruleFixture, nameof(ruleFixture));
            Check.NotNull(ontologyFixture, nameof(ontologyFixture));

            _ruleManager = ruleFixture.RuleManager;
            _ontologyManager = ontologyFixture.OntologyManager;
        }

        public static IEnumerable<object[]> MockTrueData
        {
            get
            {
                var knownFacts = new List<Fact>
                {
                    new ScalarFact("Lá", "Vàng"),
                    new ScalarFact("Thân", "Ún")
                };

                yield return new object[]
                {
                    new IE.Request("Benh", IE.RequestType.IndividualFact, knownFacts),
                    new List<Fact> {new IndividualFact("Benh", "BenhVangLa")}
                };

                yield return new object[]
                {
                    new IE.Request("Đầu lá", IE.RequestType.ScalarFact, knownFacts),
                    new List<Fact> {new ScalarFact("Đầu lá", "Cháy")}
                };
            }
        }

        public static IEnumerable<object[]> MockFalseData
        {
            get
            {
                var knownFacts = new List<Fact> {new ScalarFact("Lá", "Vàng")};

                yield return new object[]
                {
                    new IE.Request("Benh", IE.RequestType.IndividualFact, knownFacts),
                    new List<Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
                    {
                        new Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>(0.5,
                            new List<Fact> {new ScalarFact("Thân", "Ún")},
                            new List<Fact> {new IndividualFact("Benh", "BenhVangLa")})
                    }
                };

                yield return new object[]
                {
                    new IE.Request("FakeFact", IE.RequestType.IndividualFact),
                    null
                };
            }
        }

        public static IEnumerable<object[]> MockFalseThenTrueData
        {
            get
            {
                var knownFacts = new List<Fact> {new ScalarFact("Lá", "Vàng")};

                yield return new object[]
                {
                    new IE.Request("Benh", IE.RequestType.IndividualFact, knownFacts),
                    new List<Fact> {new ScalarFact("Thân", "Ún")},
                    new List<Fact> {new IndividualFact("Benh", "BenhVangLa")}
                };

                yield return new object[]
                {
                    new IE.Request("HienTuongBenh", IE.RequestType.IndividualFact, knownFacts),
                    new List<Fact> {new ScalarFact("Thân", "Ún")},
                    new List<Fact> {new IndividualFact("HienTuongBenh", "HienTuongThieuDam")}
                };
            }
        }

        [NotNull]
        public IE.IInferenceEngine LoadEngine([NotNull] IRuleManager ruleManager,
            [NotNull] IOntologyManager ontologyManager)
        {
            Check.NotNull(ruleManager, nameof(ruleManager));
            Check.NotNull(ontologyManager, nameof(ontologyManager));

            return new IE.Engine(ruleManager, ontologyManager);
        }

        [Theory]
        [MemberData(nameof(MockTrueData))]
        public void InferTrue([NotNull] IE.Request request, [NotNull] IReadOnlyCollection<Fact> expectedFacts)
        {
            Check.NotNull(request, nameof(request));
            Check.NotEmpty(expectedFacts, nameof(expectedFacts));

            var engine = LoadEngine(_ruleManager, _ontologyManager);

            var actualResultFacts = engine.Infer(request);
            Assert.True(expectedFacts.ScrambledEqual(actualResultFacts));
        }

        [Theory]
        [MemberData(nameof(MockFalseData))]
        public void InferFalse([NotNull] IE.Request request,
            [CanBeNull] IReadOnlyList<Tuple<double, IReadOnlyCollection<Fact>, IReadOnlyCollection<Fact>>>
                expectedIncompleteFacts)
        {
            Check.NotNull(request, nameof(request));

            var engine = LoadEngine(_ruleManager, _ontologyManager);

            var resultFacts = engine.Infer(request);
            Assert.Null(resultFacts);

            if (expectedIncompleteFacts == null) return;

            var actualIncompleteFacts = engine.GetIncompleteFacts(request).ToList();

            Assert.Equal(expectedIncompleteFacts.Count, actualIncompleteFacts.Count);
            for (var i = 0; i < expectedIncompleteFacts.Count; ++i)
            {
                Assert.Equal(expectedIncompleteFacts[i].Item1, actualIncompleteFacts[i].Item1, 15);
                Assert.True(expectedIncompleteFacts[i].Item2.ScrambledEqual(actualIncompleteFacts[i].Item2));
                Assert.True(expectedIncompleteFacts[i].Item3.ScrambledEqual(actualIncompleteFacts[i].Item3));
            }
        }

        [Theory]
        [MemberData(nameof(MockFalseThenTrueData))]
        public void InferFalseThenTrue([NotNull] IE.Request request, [NotNull] IReadOnlyCollection<Fact> complementFacts,
            [NotNull] IReadOnlyCollection<Fact> expectedResultFacts)
        {
            Check.NotNull(request, nameof(request));
            Check.NotEmpty(complementFacts, nameof(complementFacts));
            Check.NotEmpty(expectedResultFacts, nameof(expectedResultFacts));

            var engine = LoadEngine(_ruleManager, _ontologyManager);

            var actualResultFacts = engine.Infer(request);
            Assert.Null(actualResultFacts);

            engine.AddFactsToKnown(complementFacts);

            actualResultFacts = engine.Infer(request);
            Assert.True(expectedResultFacts.ScrambledEqual(actualResultFacts));
        }
    }
}