using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class AssignStmt : Stmt
    {
        [NotNull] private readonly string _ident;

        [NotNull] private readonly Expr _right;

        public AssignStmt([NotNull] string ident, [NotNull] Expr right)
        {
            Check.NotEmpty(ident, nameof(ident));
            Check.NotNull(right, nameof(right));

            _ident = ident;
            _right = right;
        }
    }
}