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
        }

        [NotNull]
        public IReadOnlyList<T> Symbols => _symbols;

        public bool Add([NotNull] T symbol)
        {
            if (_symbols.Any(s => s.Equals(symbol))) return false;

            _symbols.Add(symbol);

            return true;
        }
    }
}