using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.InferenceEngine
{
    public enum RequestType
    {
        IndividualFact
    }

    public class Request
    {
        [JsonConstructor]
        public Request([NotNull] Problem problem, RequestType requestType, int? totalGoals)
        {
            Check.NotNull(problem, nameof(problem));

            Problem = problem;
            RequestType = requestType;
            TotalGoals = totalGoals;
        }

        public Request(
            [NotNull] Problem problem,
            RequestType requestType,
            int? totalGoals,
            [CanBeNull] IReadOnlyCollection<Fact> knownFacts)
        {
            Check.NotNull(problem, nameof(problem));

            Problem = problem;
            RequestType = requestType;
            KnownFacts = knownFacts;
        }

        [NotNull]
        public Problem Problem { get; }

        public RequestType RequestType { get; }

        public int? TotalGoals { get; }

        [CanBeNull]
        public IReadOnlyCollection<Fact> KnownFacts { get; }
    }
}