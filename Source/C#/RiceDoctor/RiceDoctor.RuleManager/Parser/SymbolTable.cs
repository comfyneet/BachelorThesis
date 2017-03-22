using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace RiceDoctor.RuleManager
{
    public class SymbolTable<T>
    {
        [NotNull] private readonly List<T> _symbols;

        public SymbolTable()
        {
            _symbols = new List<T>();
            Symbols = _symbols.AsReadOnly();
        }

        [NotNull]
        public IReadOnlyList<T> Symbols { get; private set; }

        public bool Add([NotNull] T symbol)
        {
            if (_symbols.Any(s => s.Equals(symbol))) return false;

            _symbols.Add(symbol);
            Symbols = _symbols.AsReadOnly();

            return true;
        }
    }
}