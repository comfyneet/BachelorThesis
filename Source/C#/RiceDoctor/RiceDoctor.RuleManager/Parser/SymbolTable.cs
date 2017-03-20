using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace RiceDoctor.RuleManager
{
    public class SymbolTable
    {
        [NotNull] private readonly List<Fact> _symbols;

        public SymbolTable()
        {
            _symbols = new List<Fact>();
            Symbols = _symbols.AsReadOnly();
        }

        [NotNull]
        public IReadOnlyCollection<Fact> Symbols { get; private set; }

        public bool Add([NotNull] Fact symbol)
        {
            if (_symbols.Any(symbol.Equals)) return false;

            _symbols.Add(symbol);
            Symbols = _symbols.AsReadOnly();

            return true;
        }
    }
}