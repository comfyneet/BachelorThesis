using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class IdentExpr<T> : Expr<T>
    {
        [NotNull] private readonly T _ident;

        public IdentExpr([NotNull] T ident)
        {
            Check.NotNull(ident, nameof(ident));

            _ident = ident;
        }

        public override bool Evaluate(RuntimeContext<T> context)
        {
            return context[_ident];
        }
    }
}