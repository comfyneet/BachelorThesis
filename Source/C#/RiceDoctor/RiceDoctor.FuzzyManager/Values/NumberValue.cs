namespace RiceDoctor.FuzzyManager
{
    public class NumberValue : IValue
    {
        public NumberValue(double value)
        {
            Value = value;
        }

        public double Value { get; }

        public ValueType Type => ValueType.Number;
    }
}