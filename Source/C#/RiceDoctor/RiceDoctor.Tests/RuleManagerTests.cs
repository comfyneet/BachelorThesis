using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using Xunit;

namespace RiceDoctor.Tests
{
    [Collection("Test collection")]
    public class RuleManagerTests : IClassFixture<RuleFixture>
    {
        [NotNull] private readonly IRuleManager _manager;

        public RuleManagerTests([NotNull] RuleFixture fixture)
        {
            Check.NotNull(fixture.RuleManager, nameof(fixture), nameof(fixture.RuleManager));

            _manager = fixture.RuleManager;
        }

        [NotNull]
        public static IEnumerable<object[]> MockData
        {
            get
            {
                // https://youtu.be/VnZLRrJYa2I
                yield return new object[]
                {
                    new List<Implicant<int>>
                    {
                        new Implicant<int>(0, new List<bool?> {false, false, false, false}),
                        new Implicant<int>(4, new List<bool?> {false, true, false, false}),
                        new Implicant<int>(5, new List<bool?> {false, true, false, true}),
                        new Implicant<int>(7, new List<bool?> {false, true, true, true}),
                        new Implicant<int>(8, new List<bool?> {true, false, false, false}),
                        new Implicant<int>(11, new List<bool?> {true, false, true, true}),
                        new Implicant<int>(12, new List<bool?> {true, true, false, false}),
                        new Implicant<int>(15, new List<bool?> {true, true, true, true})
                    },
                    new List<Implicant<int>>
                    {
                        new Implicant<int>(new List<int> {0, 4, 8, 12}, new List<bool?> {null, null, false, false}),
                        new Implicant<int>(new List<int> {5, 7}, new List<bool?> {false, true, null, true}),
                        new Implicant<int>(new List<int> {11, 15}, new List<bool?> {true, null, true, true})
                    }
                };

                // https://youtu.be/l1jgq0R5EwQ
                yield return new object[]
                {
                    new List<Implicant<int>>
                    {
                        new Implicant<int>(0, new List<bool?> {false, false, false, false}),
                        new Implicant<int>(1, new List<bool?> {false, false, false, true}),
                        new Implicant<int>(3, new List<bool?> {false, false, true, true}),
                        new Implicant<int>(7, new List<bool?> {false, true, true, true}),
                        new Implicant<int>(8, new List<bool?> {true, false, false, false}),
                        new Implicant<int>(9, new List<bool?> {true, false, false, true}),
                        new Implicant<int>(11, new List<bool?> {true, false, true, true}),
                        new Implicant<int>(15, new List<bool?> {true, true, true, true})
                    },
                    new List<Implicant<int>>
                    {
                        new Implicant<int>(new List<int> {0, 1, 8, 9}, new List<bool?> {null, false, false, null}),
                        new Implicant<int>(new List<int> {3, 7, 11, 15}, new List<bool?> {null, null, true, true})
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(MockData))]
        public void MinimizeImplicants([NotNull] IReadOnlyList<Implicant<int>> implicants,
            [NotNull] IReadOnlyList<Implicant<int>> expectedPrimeImplicants)
        {
            Check.NotEmpty(implicants, nameof(implicants));
            Check.NotEmpty(expectedPrimeImplicants, nameof(expectedPrimeImplicants));

            var minimizedImplicants = QuineMcCluskey<int>.Minimize(implicants);
            Assert.True(minimizedImplicants.ScrambledEqual(expectedPrimeImplicants));
        }
    }
}