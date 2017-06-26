using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class NumberExpr : Expr
    {
        [NotNull] private readonly NumberValue _value;

        public NumberExpr([NotNull] NumberValue value)
        {
            Check.NotNull(value, nameof(value));

            _value = value;
        }

        public override IValue Evaluate(IDictionary<string, NumberValue> memory)
        {
            return _value;
        }
    }
}