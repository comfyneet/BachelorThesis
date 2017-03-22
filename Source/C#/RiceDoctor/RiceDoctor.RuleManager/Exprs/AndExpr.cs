using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class AndExpr<T> : Expr<T>
    {
        [NotNull] private readonly Expr<T> _left;

        [NotNull] private readonly Expr<T> _right;

        public AndExpr([NotNull] Expr<T> left, [NotNull] Expr<T> right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            _left = left;
            _right = right;
        }

        public override bool Evaluate(RuntimeContext<T> context)
        {
            return _left.Evaluate(context) && _right.Evaluate(context);
        }
    }
}