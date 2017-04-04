using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class FunctionStmt : Stmt
    {
        [NotNull] private readonly CompoundStmt _body;

        [NotNull] private readonly string _ident;

        private readonly int _paramCount;

        public FunctionStmt([NotNull] string ident, int paramCount, [NotNull] CompoundStmt body)
        {
            Check.NotEmpty(ident, nameof(ident));
            Check.NotNull(body, nameof(body));

            _ident = ident;
            _paramCount = paramCount;
            _body = body;
        }
    }
}