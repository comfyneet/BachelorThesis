using System;

namespace RiceDoctor.Shared
{
    public class ConsoleLogger
    {
        public void Log(object source, LogEventArgs e)
        {
            Check.NotNull(e, nameof(e));

            Console.WriteLine(e.ToString());
        }
    }
}