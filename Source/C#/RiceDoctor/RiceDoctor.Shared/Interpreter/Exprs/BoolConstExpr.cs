namespace RiceDoctor.Shared
{
    public class BoolConstExpr : Expr
    {
        private readonly bool _value;

        public BoolConstExpr(bool value)
        {
            _value = value;
            Type = ExprType.Bool;
        }
    }
}