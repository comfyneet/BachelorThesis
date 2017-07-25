using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            Logger.OnLog += logger.Log;

            if (args.Length > 0)
            {
                if (args[0] == "-crawl")
                {
                    Task.Run(async () => await RunWebCrawler()).GetAwaiter().GetResult();
                    goto Success;
                }
                if (args[0] == "-mine")
                {
                    if (args.Length == 1) goto Error;
                    bool needToTrain;
                    if (args[1] == "-train") needToTrain = true;
                    else if (args[1] == "-notrain") needToTrain = false;
                    else goto Error;

                    Task.Run(async () => await RunOntologyMiner(needToTrain)).GetAwaiter().GetResult();
                    goto Success;
                }
            }

            Error:
            Logger.Log("Need arguments.");

            Success:
            ;
        }

        public static async Task RunWebCrawler()
        {
            var newArticleCount = 0;

            using (var context = new RiceContext())
            {
                foreach (var website in context.Websites
                    .Include(nameof(Website.Categories))
                    .Include(nameof(Website.Articles)))
                {
                    var newArticles = await WebCrawler.CrawlAsync(website);
                    var awaitToAddArticles = newArticles
                        .Where(newArticle => context.Articles.All(
                            existingArticle => !existingArticle.Url.Equals(newArticle.Url)))
                        .ToList();

                    newArticleCount += awaitToAddArticles.Count;

                    if (website.Articles == null) website.Articles = new List<Article>();
                    website.Articles.AddRange(awaitToAddArticles);
                    await context.SaveChangesAsync();
                }
            }

            Logger.Log(newArticleCount == 0
                ? "FINISHED: He thong khong tim thay tin bai moi."
                : $"FINISHED: Da them {newArticleCount} tin bai vao he thong.");
        }

        public static async Task RunOntologyMiner(bool needToTrain)
        {
            List<Article> articles;
            using (var context = new RiceContext())
            {
                articles = await context.Articles.ToListAsync();
            }

            var miner = new OntologyMiner(Manager.Instance, articles);
            if (needToTrain) miner.Train();
            miner.Segment();
        }
    }
}