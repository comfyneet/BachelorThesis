using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.ConversationalAgent
{
    public class Question
    {
        public Question(int weight, [NotNull] string pattern, [NotNull] IReadOnlyCollection<Answer> answers)
        {
            Check.NotEmpty(pattern, nameof(pattern));
            Check.NotEmpty(answers, nameof(answers));

            Weight = weight;
            Pattern = pattern;
            Answers = answers;
        }

        public int Weight { get; }

        [NotNull]
        public string Pattern { get; }


        [NotNull]
        public IReadOnlyCollection<Answer> Answers { get; }
    }
}