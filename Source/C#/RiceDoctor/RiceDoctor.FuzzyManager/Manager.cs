using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class Manager : IFuzzyManager
    {
        public Manager([NotNull] string fuzzyData)
        {
            Check.NotEmpty(fuzzyData, nameof(fuzzyData));

            var lexer = new FuzzyLexer(fuzzyData);
            var parser = new FuzzyParser(lexer);

            var result = parser.Parse();
            Functions = result.Item1;
            Variables = result.Item2;
        }

        public IReadOnlyCollection<FunctionSymbol> Functions { get; }

        public IReadOnlyCollection<VariableSymbol> Variables { get; }
    }
}