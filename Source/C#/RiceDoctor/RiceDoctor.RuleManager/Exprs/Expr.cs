using JetBrains.Annotations;

namespace RiceDoctor.RuleManager
{
    public abstract class Expr
    {
        public abstract bool Evaluate([NotNull] RuntimeContext context);
    }
}