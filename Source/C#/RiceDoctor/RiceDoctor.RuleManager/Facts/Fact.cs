using JetBrains.Annotations;

namespace RiceDoctor.RuleManager
{
    public abstract class Fact
    {
        [NotNull]
        public abstract string LValue { get; }

        public abstract override bool Equals(object obj);

        public abstract override int GetHashCode();
    }
}