using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class OutputStmt : Stmt
    {
        [NotNull] private readonly Expr _value;

        public OutputStmt([NotNull] Expr value)
        {
            Check.NotNull(value, nameof(value));

            _value = value;
        }

        public override void Execute(IDictionary<string, NumberValue> memory)
        {
            Check.NotNull(memory, nameof(memory));

            memory["OUTPUT"] = (NumberValue) _value.Evaluate(memory);
        }
    }
}