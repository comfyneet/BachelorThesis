using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    public class WebCrawler
    {
        static WebCrawler()
        {
            ImagePath = Path.Combine(
                AppContext.BaseDirectory,
                @"..\..\..\..\RiceDoctor.WebApp\wwwroot\images");
        }

        public static string ImagePath { get; set; }

        [NotNull]
        public static async Task<IReadOnlyCollection<Article>> CrawlAsync([NotNull] Website website)
        {
            Check.NotNull(website, nameof(website));

            var articles = new List<Article>();

            var web = new HtmlWeb();
            var articleTasksAllCategories = website.Categories
                .Select(c => new Tuple<Category, Task<HtmlDocument>>(c, web.LoadFromWebAsync(c.Url)))
                .Select(async pair => CrawlArticlesByCategoryAsync(pair.Item1, await pair.Item2));
            foreach (var articleTasks1Category in articleTasksAllCategories)
            foreach (var articleTask in await articleTasks1Category)
            {
                var article = await articleTask;
                if (article != null) articles.Add(article);
            }

            return articles;
        }

        [NotNull]
        private static IReadOnlyCollection<Task<Article>> CrawlArticlesByCategoryAsync(
            [NotNull] Category category,
            [NotNull] HtmlDocument categoryDocument)
        {
            Check.NotNull(category, nameof(category));
            Check.NotNull(categoryDocument, nameof(categoryDocument));

            var articleUrls = categoryDocument
                .DocumentNode
                .SelectNodes(category.ArticleXPath)
                .Select(node =>
                {
                    var uriString = WebUtility.HtmlDecode(node.Attributes["href"].Value).Trim();

                    if (!(Uri.TryCreate(uriString, UriKind.Absolute, out Uri uri) &&
                          (uri.Scheme == "http" || uri.Scheme == "https")))
                        uri = new Uri(new Uri(category.Url), uriString);

                    return uri.AbsoluteUri;
                });

            var web = new HtmlWeb();
            var articleTasks = articleUrls
                .Select(url => new Tuple<string, Task<HtmlDocument>>(url, web.LoadFromWebAsync(url)))
                .Select(async pair => await CrawlArticleAsync(pair.Item1, await pair.Item2, category)).ToList();

            return articleTasks;
        }

        [CanBeNull]
        public static async Task<Article> CrawlArticleAsync(
            [NotNull] string articleUrl,
            [NotNull] HtmlDocument articleDocument,
            [NotNull] Category category)
        {
            Check.NotEmpty(articleUrl, nameof(articleUrl));
            Check.NotNull(articleDocument, nameof(articleDocument));
            Check.NotNull(category, nameof(category));

            var title = WebUtility.HtmlDecode(articleDocument.DocumentNode.SelectSingleNode(category.TitleXPath)
                .InnerText);
            var content = WebUtility.HtmlDecode(articleDocument.DocumentNode.SelectSingleNode(category.ContentXPath)
                .InnerText);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content)) return null;

            string imageFilename = null;
            var imageNode = articleDocument.DocumentNode.SelectSingleNode(category.ContentXPath + "//img[1]");
            if (imageNode != null)
            {
                var uriString = WebUtility.HtmlDecode(imageNode.Attributes["src"].Value).Trim();
                if (!(Uri.TryCreate(uriString, UriKind.Absolute, out var uri) &&
                      (uri.Scheme == "http" || uri.Scheme == "https")))
                    uri = new Uri(new Uri(category.Url), uriString);

                var image = await CrawlImage(uri);
                var info = new FileInfo(Path.GetFileName(uri.AbsolutePath));
                imageFilename = "article_" + Guid.NewGuid() + info.Extension;
                File.WriteAllBytes(Path.Combine(ImagePath, imageFilename), image);
            }

            var article = new Article
            {
                Title = title.Trim(),
                Content = content.Trim(),
                Url = articleUrl,
                Image = imageFilename,
                RetrievedDate = DateTime.Now
            };

            return article;
        }

        [NotNull]
        private static async Task<byte[]> CrawlImage([NotNull] Uri imageUri)
        {
            Check.NotNull(imageUri, nameof(imageUri));

            using (var client = new HttpClient())
            using (var response = await client.GetAsync(imageUri))
            using (var content = response.Content)
            {
                var image = await content.ReadAsByteArrayAsync();
                return image;
            }
        }
    }
}