using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.InferenceEngine
{
    public enum ResponseType
    {
        AskGuessableFacts,
        ShowCompleteResults,

        ShowIncompleteResults
        //ShowNoResults
    }

    public class Response
    {
        private Response()
        {
        }

        [CanBeNull]
        public IReadOnlyDictionary<Fact, double> ResultFacts { get; private set; }

        [CanBeNull]
        public IReadOnlyCollection<Fact> GuessableFacts { get; private set; }

        public ResponseType Type { get; private set; }

        public static Response AskGuessableFacts([NotNull] IReadOnlyCollection<Fact> guessableFacts)
        {
            Check.NotNull(guessableFacts, nameof(guessableFacts));

            return new Response
            {
                Type = ResponseType.AskGuessableFacts,
                GuessableFacts = guessableFacts
            };
        }

        public static Response ShowInferredResults([NotNull] IReadOnlyDictionary<Fact, double> resultFacts)
        {
            Check.NotNull(resultFacts, nameof(resultFacts));

            return new Response
            {
                Type = ResponseType.ShowCompleteResults,
                ResultFacts = resultFacts
            };
        }

        public static Response ShowIncompleteResults([NotNull] IReadOnlyDictionary<Fact, double> resultFacts)
        {
            Check.NotNull(resultFacts, nameof(resultFacts));

            return new Response
            {
                Type = ResponseType.ShowIncompleteResults,
                ResultFacts = resultFacts
            };
        }
    }
}