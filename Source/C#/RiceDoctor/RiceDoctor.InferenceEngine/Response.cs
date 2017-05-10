using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;
using static RiceDoctor.InferenceEngine.ResponseType;

namespace RiceDoctor.InferenceEngine
{
    public enum ResponseType
    {
        GuessableFact,
        InferredResults,
        NoResults
    }

    public class Response
    {
        public Response()
        {
            Type = NoResults;
        }

        public Response([NotNull] IReadOnlyCollection<Fact> results)
        {
            Check.NotNull(results, nameof(results));

            Type = InferredResults;
            Results = results;
        }

        public Response([NotNull] Fact guessableFact)
        {
            Check.NotNull(guessableFact, nameof(guessableFact));

            Type = ResponseType.GuessableFact;
            GuessableFact = guessableFact;
        }

        [CanBeNull]
        public IReadOnlyCollection<Fact> Results { get; }

        [CanBeNull]
        public Fact GuessableFact { get; }

        public ResponseType Type { get; }
    }
}