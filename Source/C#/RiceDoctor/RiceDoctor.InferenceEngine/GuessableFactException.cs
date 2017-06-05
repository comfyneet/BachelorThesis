using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.InferenceEngine
{
    public class GuessableFactException : Exception
    {
        public GuessableFactException([NotNull] IReadOnlyCollection<Fact> facts)
        {
            Check.NotNull(facts, nameof(facts));

            Facts = facts;
        }

        [NotNull]
        public IReadOnlyCollection<Fact> Facts { get; }
    }
}