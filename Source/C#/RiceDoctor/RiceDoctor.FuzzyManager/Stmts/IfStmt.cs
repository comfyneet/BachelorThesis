using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class IfStmt : Stmt
    {
        [NotNull] private readonly Expr _condition;

        [NotNull] private readonly OutputStmt _outputStmt;

        public IfStmt([NotNull] Expr condition, [NotNull] OutputStmt outputStmt)
        {
            Check.NotNull(condition, nameof(condition));
            Check.NotNull(outputStmt, nameof(outputStmt));

            _condition = condition;
            _outputStmt = outputStmt;
        }

        public override void Execute(IDictionary<string, NumberValue> memory)
        {
            Check.NotNull(memory, nameof(memory));

            var result = ((BoolValue) _condition.Evaluate(memory)).Value;
            if (result) _outputStmt.Execute(memory);
        }
    }
}