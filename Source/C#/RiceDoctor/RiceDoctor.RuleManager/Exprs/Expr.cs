using JetBrains.Annotations;

namespace RiceDoctor.RuleManager
{
    public abstract class Expr<T>
    {
        public abstract bool Evaluate([NotNull] RuntimeContext<T> context);
    }
}