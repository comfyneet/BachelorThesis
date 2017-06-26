using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class FunctionStmt : Stmt
    {
        [NotNull] private readonly IReadOnlyCollection<IfStmt> _body;


        public FunctionStmt([NotNull] IReadOnlyCollection<IfStmt> body)
        {
            Check.NotNull(body, nameof(body));

            _body = body;
        }

        public override void Execute(IDictionary<string, NumberValue> memory)
        {
            Check.NotNull(memory, nameof(memory));

            foreach (var ifStmt in _body)
                ifStmt.Execute(memory);
        }
    }
}