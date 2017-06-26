using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public enum BinaryOp
    {
        Equal,
        NotEqual,
        Less,
        LessEqual,
        GreaterEqual,
        Greater,
        Or,
        And,
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo
    }

    public class BinaryExpr : Expr
    {
        [NotNull] private readonly Expr _left;

        private readonly BinaryOp _op;

        [NotNull] private readonly Expr _right;

        public BinaryExpr([NotNull] Expr left, BinaryOp op, [NotNull] Expr right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            _left = left;
            _op = op;
            _right = right;
        }

        public override IValue Evaluate(IDictionary<string, NumberValue> memory)
        {
            var leftValue = _left.Evaluate(memory);
            var rightValue = _right.Evaluate(memory);

            if (leftValue.Type == ValueType.Boolean)
            {
                var leftBool = ((BoolValue) leftValue).Value;
                var rightBool = ((BoolValue) rightValue).Value;

                bool value;
                switch (_op)
                {
                    case BinaryOp.Equal:
                        value = leftBool == rightBool;
                        break;
                    case BinaryOp.NotEqual:
                        value = leftBool != rightBool;
                        break;
                    case BinaryOp.Or:
                        value = leftBool || rightBool;
                        break;
                    case BinaryOp.And:
                        value = leftBool && rightBool;
                        break;
                    default:
                        throw new InvalidOperationException(nameof(BinaryExpr));
                }

                return new BoolValue(value);
            }

            var leftNumber = ((NumberValue) leftValue).Value;
            var rightNumber = ((NumberValue) rightValue).Value;

            IValue returnValue;
            switch (_op)
            {
                case BinaryOp.Equal:
                    returnValue = new BoolValue(leftNumber.Equals3DigitPrecision(rightNumber));
                    break;
                case BinaryOp.NotEqual:
                    returnValue = new BoolValue(!leftNumber.Equals3DigitPrecision(rightNumber));
                    break;
                case BinaryOp.Less:
                    returnValue = new BoolValue(leftNumber < rightNumber);
                    break;
                case BinaryOp.LessEqual:
                    returnValue = new BoolValue(leftNumber <= rightNumber);
                    break;
                case BinaryOp.GreaterEqual:
                    returnValue = new BoolValue(leftNumber >= rightNumber);
                    break;
                case BinaryOp.Greater:
                    returnValue = new BoolValue(leftNumber > rightNumber);
                    break;
                case BinaryOp.Add:
                    returnValue = new NumberValue(leftNumber + rightNumber);
                    break;
                case BinaryOp.Subtract:
                    returnValue = new NumberValue(leftNumber - rightNumber);
                    break;
                case BinaryOp.Multiply:
                    returnValue = new NumberValue(leftNumber * rightNumber);
                    break;
                case BinaryOp.Divide:
                    returnValue = new NumberValue(leftNumber / rightNumber);
                    break;
                default:
                    throw new InvalidOperationException(nameof(BinaryExpr));
            }

            return returnValue;
        }
    }
}