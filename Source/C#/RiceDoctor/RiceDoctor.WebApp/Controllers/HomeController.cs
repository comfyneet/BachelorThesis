using Microsoft.AspNetCore.Mvc;

namespace RiceDoctor.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Trang giới thiệu về khoá luận tốt nghiệp";

            return View();
        }

        public IActionResult Error(string error)
        {
            return View(error);
        }
    }
}