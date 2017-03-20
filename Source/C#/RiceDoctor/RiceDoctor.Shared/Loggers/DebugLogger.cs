using System.Diagnostics;

namespace RiceDoctor.Shared
{
    public class DebugLogger
    {
        public void Log(object source, LogEventArgs e)
        {
            Check.NotNull(e, nameof(e));

            Debug.WriteLine(e.ToString());
        }
    }
}