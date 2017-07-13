using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
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

            return View(websites);
        }
    }
}