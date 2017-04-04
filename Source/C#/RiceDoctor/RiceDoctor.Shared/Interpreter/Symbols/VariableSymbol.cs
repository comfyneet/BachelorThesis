using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class VariableSymbol : Symbol
    {
        public VariableSymbol([NotNull] string ident, ExprType type) : base(ident, type)
        {
        }
    }
}