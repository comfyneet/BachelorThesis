using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    public class WebCrawler
    {
        [NotNull]
        public static async Task<IReadOnlyCollection<Article>> CrawlAsync([NotNull] Website website)
        {
            Check.NotNull(website, nameof(website));

            var articles = new List<Article>();
            var categoryArticleTasks = website.Categories.Select(CrawlCategoryArticlesAsync).ToList();
            foreach (var categoryArticleTask in categoryArticleTasks)
            {
                var categoryArticles = await categoryArticleTask;
                foreach (var article in categoryArticles)
                    articles.Add(await article);
            }

            website.Articles = articles;

            return website.Articles;
        }

        public static async Task<List<Task<Article>>> CrawlCategoryArticlesAsync([NotNull] Category category)
        {
            Check.NotNull(category, nameof(category));

            var categoryUri = new Uri(category.Url);

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(category.Url);

            var articleUrls = doc
                .DocumentNode
                .SelectNodes(category.ArticleXPath)
                .Select(node =>
                {
                    var text = WebUtility.HtmlDecode(node.Attributes["href"].Value).Trim();
                    var articleUri = new Uri(categoryUri, text);
                    return articleUri.AbsoluteUri;
                });

            var articleTasks = articleUrls.Select(url => CrawlArticleAsync(url, category)).ToList();

            return articleTasks;
        }

        public static async Task<Article> CrawlArticleAsync([NotNull] string articleUrl, [NotNull] Category category)
        {
            Check.NotEmpty(articleUrl, nameof(articleUrl));
            Check.NotNull(category, nameof(category));

            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(articleUrl);

            var title = WebUtility.HtmlDecode(doc.DocumentNode
                    .SelectSingleNode(category.TitleXPath).InnerText)
                .Trim();
            var content = WebUtility.HtmlDecode(doc.DocumentNode
                    .SelectSingleNode(category.ContentXPath).InnerText)
                .Trim();

            var article = new Article
            {
                Title = title,
                Content = content,
                Url = articleUrl,
                RetrievedDate = DateTime.Now
            };

            return article;
        }
    }
}