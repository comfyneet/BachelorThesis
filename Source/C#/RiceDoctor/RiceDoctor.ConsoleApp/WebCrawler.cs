using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    public class WebCrawler
    {
        [NotNull]
        public static IReadOnlyCollection<Article> Crawl([NotNull] Website website)
        {
            Check.NotNull(website, nameof(website));

            var articles = new List<Article>();
            var web = new HtmlWeb();

            foreach (var category in website.Categories)
            {
                var categoryUri = new Uri(category.Url);

                var articleUrls = web
                    .Load(category.Url)
                    .DocumentNode
                    .SelectNodes(category.ArticleXPath)
                    .Select(node =>
                    {
                        var text = WebUtility.HtmlDecode(node.Attributes["href"].Value).Trim();
                        var articleUri = new Uri(categoryUri, text);
                        return articleUri.AbsoluteUri;
                    });

                foreach (var articleUrl in articleUrls)
                {
                    var doc = web.Load(articleUrl);
                    var title = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode(category.TitleXPath).InnerText)
                        .Trim();
                    var content = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode(category.ContentXPath)
                        .InnerText).Trim();

                    var article = new Article
                    {
                        Title = title,
                        Content = content,
                        RetrievedDate = DateTime.Now,
                        Website = website
                    };
                    articles.Add(article);
                }
            }

            website.Articles = articles;

            return articles;
        }
    }
}