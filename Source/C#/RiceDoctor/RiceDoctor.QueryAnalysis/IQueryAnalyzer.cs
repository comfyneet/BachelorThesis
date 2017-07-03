using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.QueryAnalysis
{
    public interface IQueryAnalyzer
    {
        [NotNull]
        IReadOnlyCollection<Query> Queries { get; }
    }
}