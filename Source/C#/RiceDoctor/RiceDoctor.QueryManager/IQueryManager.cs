using System.Collections.Generic;
using JetBrains.Annotations;

namespace RiceDoctor.QueryManager
{
    public interface IQueryManager
    {
        [NotNull]
        IReadOnlyCollection<Query> Queries { get; }
    }
}