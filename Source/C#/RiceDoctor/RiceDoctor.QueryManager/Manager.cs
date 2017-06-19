using System.Collections.Generic;
using JetBrains.Annotations;
using RiceDoctor.Shared;

namespace RiceDoctor.QueryManager
{
    public class Manager : IQueryManager
    {
        public Manager([NotNull] string queryData)
        {
            Check.NotEmpty(queryData, nameof(queryData));

            var lexer = new QueryLexer(queryData);
            var parser = new QueryParser(lexer);
            Queries = parser.Parse();
        }

        public IReadOnlyCollection<Query> Queries { get; }
    }
}