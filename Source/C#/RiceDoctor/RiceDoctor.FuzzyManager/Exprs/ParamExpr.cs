using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class ParamExpr : Expr
    {
        [NotNull] private readonly string _name;

        public ParamExpr([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            _name = name;
        }

        public override IValue Evaluate(IDictionary<string, NumberValue> memory)
        {
            Check.NotNull(memory, nameof(memory));

            return memory[_name];
        }
    }
}