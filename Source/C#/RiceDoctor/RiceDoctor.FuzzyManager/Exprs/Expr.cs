using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.FuzzyManager
{
    public abstract class Expr
    {
        public abstract IValue Evaluate([NotNull] IDictionary<string, NumberValue> memory);
    }
}