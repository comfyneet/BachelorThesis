using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class AddUnaryExpr
    {
        [NotNull] private readonly TokenType _op;
        [NotNull] private readonly Expr _right;

        public AddUnaryExpr([NotNull] Expr right, [NotNull] TokenType op)
        {
            Check.NotNull(right, nameof(right));
            Check.NotNull(op, nameof(op));

            _right = right;
            _op = op;
        }
    }
}