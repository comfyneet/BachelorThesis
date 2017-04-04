namespace RiceDoctor.Shared
{
    public class DecimalConstExpr : Expr
    {
        private readonly decimal _value;

        public DecimalConstExpr(decimal value)
        {
            _value = value;
            Type = ExprType.Decimal;
        }
    }
}