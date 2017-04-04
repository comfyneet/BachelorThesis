using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class CompoundStmt : Stmt
    {
        [CanBeNull] private readonly IReadOnlyCollection<Stmt> _stmts;

        public CompoundStmt([CanBeNull] IReadOnlyCollection<Stmt> stmts)
        {
            _stmts = stmts;
        }
    }
}