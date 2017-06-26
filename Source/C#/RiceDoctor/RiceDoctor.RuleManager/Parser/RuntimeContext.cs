using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.RuleManager
{
    public class RuntimeContext<T>
    {
        [NotNull] private readonly Dictionary<T, bool> _memory;

        public RuntimeContext([NotNull] SymbolTable<T> symbolTable)
        {
            Check.NotNull(symbolTable, nameof(symbolTable));

            _memory = new Dictionary<T, bool>();
            foreach (var symbol in symbolTable.Symbols)
                _memory.Add(symbol, false);
        }

        public bool this[T key]
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