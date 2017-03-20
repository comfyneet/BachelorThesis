using System.IO;
using JetBrains.Annotations;

namespace RiceDoctor.Shared
{
    public class FileLogger
    {
        [NotNull] private readonly string _filePath;

        public FileLogger([NotNull] string filePath)
        {
            Check.NotNull(filePath, nameof(filePath));

            _filePath = filePath;
        }

        public void Log(object source, LogEventArgs e)
        {
            Check.NotNull(e, nameof(e));

            using (var writer = File.AppendText(_filePath))
            {
                writer.WriteLine(e.ToString());
            }
        }
    }
}