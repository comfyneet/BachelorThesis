using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiceDoctor.DatabaseManager;
using RiceDoctor.Shared;

namespace RiceDoctor.WebApp.Controllers
{
    public class AdminController : Controller
    {
        [NotNull] private readonly RiceContext _context;

        public AdminController([NotNull] RiceContext context)
        {
            Check.NotNull(context, nameof(context));

            _context = context;
        }

        public IActionResult Index()
        {
            var websites = _context.Websites.ToList();

            return View("Index", websites);
        }

        public IActionResult Articles()
        {
            var articles = _context.Articles.Include(nameof(DatabaseManager.Article.Website)).ToList();

            return View("Articles", articles);
        }

        public IActionResult Website(int id)
        {
            var website = _context.Websites
                .Include(nameof(DatabaseManager.Website.Categories))
                .Include(nameof(DatabaseManager.Website.Articles))
                .FirstOrDefault(w => w.Id == id);
            if (website == null)
                return RedirectToAction("Error", "Home", new {error = $"WebsiteId \"{id}\" not found."});

            return View("Website", website);
        }

        public IActionResult CreateWebsite()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PostCreateWebsite(Website website)
        {
            if (!ModelState.IsValid) return View("Index");

            website.Name = website.Name.Trim();
            website.Url = website.Url.Trim();

            if (website.Categories != null)
                foreach (var category in website.Categories)
                {
                    category.Url = category.Url.Trim();
                    category.ArticleXPath = category.ArticleXPath.Trim();
                    category.TitleXPath = category.TitleXPath.Trim();
                    category.ContentXPath = category.ContentXPath.Trim();
                }

            _context.Websites.Add(website);
            _context.SaveChanges();

            ViewData["WebsiteCreated"] = true;

            return Website(website.Id);
        }

        [HttpPost]
        public IActionResult EditWebsite(Website website)
        {
            if (!ModelState.IsValid) return View("Index");

            var tmpWebsite = _context.Websites
                .Include(nameof(DatabaseManager.Website.Categories))
                .FirstOrDefault(w => w.Id == website.Id);
            if (tmpWebsite == null)
                return RedirectToAction("Error", "Home", new {error = $"WebsiteId \"{website.Id}\" not found."});

            tmpWebsite.Name = website.Name.Trim();
            tmpWebsite.Url = website.Url.Trim();

            _context.SaveChanges();

            ViewData["WebsiteEdited"] = true;

            return Website(website.Id);
        }

        public IActionResult DeleteWebsite(int id)
        {
            var website = _context.Websites.FirstOrDefault(w => w.Id == id);

            _context.Websites.Remove(website);
            _context.SaveChanges();

            ViewData["DeletedWebsite"] = website.Name;

            return Index();
        }

        public IActionResult CreateCategory(int websiteId)
        {
            ViewData["Website"] = _context.Websites.FirstOrDefault(w => w.Id == websiteId);

            return View();
        }

        [HttpPost]
        public IActionResult PostCreateCategory(Category category)
        {
            if (!ModelState.IsValid) return View("Index");

            category.Url = category.Url.Trim();
            category.ArticleXPath = category.ArticleXPath.Trim();
            category.TitleXPath = category.TitleXPath.Trim();
            category.ContentXPath = category.ContentXPath.Trim();


            _context.Categories.Add(category);
            _context.SaveChanges();

            ViewData["CreatedCategory"] = category.Url;

            return Website(category.WebsiteId);
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.Id == id);

            _context.Categories.Remove(category);
            _context.SaveChanges();

            ViewData["DeletedCategory"] = category.Url;

            return Website(category.WebsiteId);
        }

        public IActionResult Article(int id)
        {
            var article = _context.Articles
                .Include(nameof(DatabaseManager.Article.Website))
                .FirstOrDefault(a => a.Id == id);

            return View("Article", article);
        }

        public IActionResult DeleteArticle(int id)
        {
            var article = _context.Articles.FirstOrDefault(a => a.Id == id);

            _context.Articles.Remove(article);
            _context.SaveChanges();

            ViewData["DeletedArticle"] = article.Title;

            return View("Articles");
        }
    }
}