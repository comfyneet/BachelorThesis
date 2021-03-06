﻿using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.FuzzyManager
{
    public interface IFuzzyManager
    {
        [NotNull]
        IReadOnlyCollection<FunctionSymbol> Functions { get; }

        [NotNull]
        IReadOnlyList<VariableSymbol> Variables { get; }
    }
}