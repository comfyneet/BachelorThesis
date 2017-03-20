using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class AndExpr : Expr
    {
        [NotNull] private readonly Expr _left;

        [NotNull] private readonly Expr _right;

        public AndExpr([NotNull] Expr left, [NotNull] Expr right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            _left = left;
            _right = right;
        }

        public override bool Evaluate(RuntimeContext context)
        {
            return _left.Evaluate(context) && _right.Evaluate(context);
        }
    }
}