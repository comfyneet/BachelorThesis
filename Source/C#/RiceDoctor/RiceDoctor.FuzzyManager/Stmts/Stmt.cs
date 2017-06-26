using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.FuzzyManager
{
    public abstract class Stmt
    {
        public abstract void Execute([NotNull] IDictionary<string, NumberValue> memory);
    }
}