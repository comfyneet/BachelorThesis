using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RiceDoctor.Shared;

namespace RiceDoctor.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "log.txt");
            var logger = new FileLogger(filePath);
            Logger.OnLog += logger.Log;

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}