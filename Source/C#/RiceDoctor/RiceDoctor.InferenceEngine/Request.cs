using JetBrains.Annotations;
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
        public Request([NotNull] Problem problem, RequestType requestType, int? totalGoals)
        {
            Check.NotNull(problem, nameof(problem));

            Problem = problem;
            RequestType = requestType;
            TotalGoals = totalGoals;
        }

        [NotNull]
        public Problem Problem { get; }

        public RequestType RequestType { get; }

        public int? TotalGoals { get; }
    }
}