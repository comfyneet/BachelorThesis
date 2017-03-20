using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.ConversationalAgent
{
    public class Answer
    {
        public Answer(int weight, [NotNull] string text)
        {
            Check.NotEmpty(text, nameof(text));

            Weight = weight;
            Text = text;
        }

        [NotNull]
        public string Text { get; }

        public int Weight { get; }
    }
}