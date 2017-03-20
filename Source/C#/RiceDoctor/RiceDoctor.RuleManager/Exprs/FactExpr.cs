using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class FactExpr : Expr
    {
        [NotNull] private readonly Fact _fact;

        public FactExpr([NotNull] Fact fact)
        {
            Check.NotNull(fact, nameof(fact));

            _fact = fact;
        }

        public override bool Evaluate(RuntimeContext context)
        {
            return context[_fact];
        }
    }
}