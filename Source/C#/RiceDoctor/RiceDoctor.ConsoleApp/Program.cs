using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RiceDoctor.DatabaseManager;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            Logger.OnLog += logger.Log;

            Task.Run(async () => await AsyncMain(args)).GetAwaiter().GetResult();

            Console.ReadKey();
        }

        public static async Task AsyncMain(string[] args)
        {
            using (var context = new RiceContext())
            {
                foreach (var website in context.Websites)
                {
                    var newArticles = await WebCrawler.CrawlAsync(website);
                    var awaitToAddArticles = newArticles
                        .Where(newArticle => context.Articles.All(
                            existingArticle => !existingArticle.Url.Equals(newArticle.Url)))
                        .ToList();

                    if (website.Articles == null) website.Articles = new List<Article>();
                    website.Articles.AddRange(awaitToAddArticles);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}