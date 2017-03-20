using System;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(DateTime time, Level level, [NotNull] string message)
        {
            Check.NotNull(message, nameof(message));

            Time = time;
            Level = level;
            Message = message;
        }

        public DateTime Time { get; }

        public Level Level { get; }

        [NotNull]
        public string Message { get; }

        public override string ToString()
        {
            return $"{Time:dd:MM:yyyy HH:mm}: {Level}: {Message}";
        }
    }
}