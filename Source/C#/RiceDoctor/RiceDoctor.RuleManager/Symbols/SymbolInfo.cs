using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public enum Type
    {
        Boolean,
        Number
    }

    public class SymbolInfo
    {
        public SymbolInfo([NotNull] object value, Type type)
        {
            Check.NotNull(value, nameof(value));

            Value = value;
            Type = type;
        }

        public object Value { get; }

        public Type Type { get; }
    }
}