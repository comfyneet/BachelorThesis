using System.IO;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class FileLogger
    {
        [NotNull] private static readonly object LogWriteLock = new object();

        [NotNull] private readonly string _filePath;

        public FileLogger([NotNull] string filePath)
        {
            Check.NotEmpty(filePath, nameof(filePath));

            _filePath = filePath;
        }

        public void Log(object source, LogEventArgs e)
        {
            Check.NotNull(e, nameof(e));

            lock (LogWriteLock)
            {
                var length = new FileInfo(_filePath).Length;
                if (length > 1024 * 1024) File.WriteAllText(_filePath, "");

                using (var writer = File.AppendText(_filePath))
                {
                    writer.WriteLine(e.ToString());
                }
            }
        }
    }
}