using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public enum UnaryOp
    {
        Not,
        Positive,
        Negative
    }

    public class UnaryExpr : Expr
    {
        private readonly UnaryOp _op;

        [NotNull] private readonly Expr _right;

        public UnaryExpr(UnaryOp op, [NotNull] Expr right)
        {
            Check.NotNull(right, nameof(right));

            _op = op;
            _right = right;
        }

        public override IValue Evaluate(IDictionary<string, NumberValue> memory)
        {
            var rightValue = _right.Evaluate(memory);

            if (rightValue.Type == ValueType.Boolean)
            {
                var rightBool = ((BoolValue) rightValue).Value;
                switch (_op)
                {
                    case UnaryOp.Not:
                        return new BoolValue(!rightBool);
                    default:
                        throw new InvalidOperationException(nameof(UnaryExpr));
                }
            }

            var rightNumber = ((NumberValue) rightValue).Value;
            switch (_op)
            {
                case UnaryOp.Positive:
                    return new NumberValue(+rightNumber);
                case UnaryOp.Negative:
                    return new NumberValue(-rightNumber);
                default:
                    throw new InvalidOperationException(nameof(UnaryExpr));
            }
        }
    }
}