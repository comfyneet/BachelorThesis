using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class RuntimeContext
    {
        [NotNull] private readonly Dictionary<Fact, bool> _memory;

        public RuntimeContext([NotNull] SymbolTable symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            _memory = new Dictionary<Fact, bool>();
            foreach (var fact in symbolTable.Symbols)
                _memory.Add(fact, false);
        }

        public bool this[Fact key]
        {
            get
            {
                Check.NotNull(key, nameof(key));

                return _memory[key];
            }
            set
            {
                Check.NotNull(key, nameof(key));

                if (!_memory.ContainsKey(key)) throw new KeyNotFoundException(nameof(key));

                _memory[key] = value;
            }
        }
    }
}