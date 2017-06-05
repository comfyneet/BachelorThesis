using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using static RiceDoctor.InferenceEngine.ResponseType;

namespace RiceDoctor.InferenceEngine
{
    public enum ResponseType
    {
        GuessableFacts,
        InferredResults,
        NoResults
    }

    public class Response
    {
        public Response()
        {
            Type = NoResults;
        }

        public Response([NotNull] IReadOnlyCollection<Fact> facts, ResponseType type)
        {
            Check.NotNull(facts, nameof(facts));

            Facts = facts;
            Type = type;
        }

        [CanBeNull]
        public IReadOnlyCollection<Fact> Facts { get; }

        public ResponseType Type { get; }
    }
}