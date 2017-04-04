using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class FunctionSymbol : Symbol
    {
        public FunctionSymbol([NotNull] string ident, [CanBeNull] IReadOnlyCollection<ExprType> formalParams,
            ExprType returnType)
            : base(ident, returnType)
        {
            FormalParams = formalParams;
        }

        public IReadOnlyCollection<ExprType> FormalParams { get; }
    }
}