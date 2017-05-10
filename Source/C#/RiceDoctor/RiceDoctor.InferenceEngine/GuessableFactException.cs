using System;
using JetBrains.Annotations;
using RiceDoctor.RuleManager;
using RiceDoctor.Shared;

namespace RiceDoctor.InferenceEngine
{
    public class GuessableFactException : Exception
    {
        public GuessableFactException([NotNull] Fact fact)
        {
            Check.NotNull(fact, nameof(fact));

            Fact = fact;
        }

        [NotNull]
        public Fact Fact { get; }
    }
}