namespace RiceDoctor.FuzzyManager
{
    public class BoolValue : IValue
    {
        public BoolValue(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public ValueType Type => ValueType.Boolean;
    }
}