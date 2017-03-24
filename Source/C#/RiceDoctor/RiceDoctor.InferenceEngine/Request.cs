using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.InferenceEngine
{
    public class Request
    {
        public Request([NotNull] string factName, RequestType requestType)
        {
            Check.NotEmpty(factName, nameof(factName));

            FactName = factName;
            RequestType = requestType;
        }

        public Request([NotNull] string factName, RequestType requestType,
            [NotNull] IReadOnlyCollection<Fact> knownFacts)
        {
            Check.NotEmpty(factName, nameof(factName));
            Check.NotEmpty(knownFacts, nameof(knownFacts));

            FactName = factName;
            RequestType = requestType;
            KnownFacts = knownFacts;
        }

        [NotNull]
        public string FactName { get; }

        public RequestType RequestType { get; }

        [CanBeNull]
        public IReadOnlyCollection<Fact> KnownFacts { get; }
    }
}