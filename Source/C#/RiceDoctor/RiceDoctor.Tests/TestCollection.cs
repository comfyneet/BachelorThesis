using System;
using JetBrains.Annotations;
using RiceDoctor.Shared;
using Xunit;

namespace RiceDoctor.Tests
{
    public class DebugFixture : IDisposable
    {
        [NotNull] private readonly DebugLogger _logger;

        public DebugFixture()
        {
            _logger = new DebugLogger();
            Logger.OnLog += _logger.Log;
        }

        public void Dispose()
        {
            Logger.OnLog -= _logger.Log;
        }
    }

    [CollectionDefinition("Test collection")]
    public class TestCollection : ICollectionFixture<DebugFixture>
    {
    }
}