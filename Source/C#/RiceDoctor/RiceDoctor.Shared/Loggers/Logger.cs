using System;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public delegate void LogEventHandler(object source, LogEventArgs e);

    public static class Logger
    {
        public static Level DefaultLevel { get; set; } = Level.Info;
        public static event LogEventHandler OnLog;

        public static void Log([NotNull] string message)
        {
            Log(DefaultLevel, message);
        }

        public static void Log(Level level, [NotNull] string message)
        {
            Check.NotNull(message, nameof(message));

            OnLog?.Invoke(typeof(Logger), new LogEventArgs(DateTime.Now, level, message));
        }
    }
}