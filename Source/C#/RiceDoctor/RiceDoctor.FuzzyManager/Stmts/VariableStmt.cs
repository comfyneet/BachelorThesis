using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.FuzzyManager
{
    public class VariableStmt
    {
        [NotNull]
        private readonly IReadOnlyDictionary<string, Tuple<FunctionSymbol, IReadOnlyList<NumberValue>>> _terms;

        public VariableStmt(
            [NotNull] IReadOnlyDictionary<string, Tuple<FunctionSymbol, IReadOnlyList<NumberValue>>> terms)
        {
            Check.NotNull(terms, nameof(terms));

            _terms = terms;
        }

        [NotNull]
        public IReadOnlyDictionary<string, double> Execute(double input)
        {
            var results = new Dictionary<string, double>();

            foreach (var term in _terms)
            {
                var memory = new Dictionary<string, NumberValue> {["INPUT"] = new NumberValue(input)};
                for (var i = 0; i < term.Value.Item2.Count; ++i)
                    memory[term.Value.Item1.Params[i]] = term.Value.Item2[i];

                term.Value.Item1.Stmt.Execute(memory);

                results.Add(term.Key, memory["OUTPUT"].Value);
            }

            return results;
        }
    }
}