using System.Linq;
using JetBrains.Annotations;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using RiceDoctor.DatabaseManager;
using RiceDoctor.Shared;
using RiceDoctor.WebApp.Models;

namespace RiceDoctor.WebApp.Controllers
{
    public class HomeController : Controller
    {
        [NotNull] private readonly RiceContext _context;

        public HomeController([NotNull] RiceContext context)
        {
            Check.NotNull(context, nameof(context));

            _context = context;
        }

        public IActionResult Index()
        {
            var articles = _context.Articles.OrderByDescending(a => a.RetrievedDate).Take(3).ToList();

            return View(articles);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Feedback(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Feedback", "ricedoctor.uit@gmail.com"));
                message.To.Add(new MailboxAddress("", "ricedoctor.uit@gmail.com"));
                message.Subject = $"{feedback.Subject.Trim()} from {feedback.Name.Trim()} ({feedback.Email.Trim()})";
                message.Body = new TextPart("plain") {Text = feedback.Message.Trim()};

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect("smtp.gmail.com", 587);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate("ricedoctor.uit@gmail.com", "uit.edu.vn");
                    client.Send(message);
                    client.Disconnect(true);
                }

                ViewData["Message"] =
                    "The mail has been sent to <a href=\"mailto:ricedoctor.uit@gmail.com\">ricedoctor.uit@gmail.com</a> successfully.";
            }

            return View("Contact");
        }

        public IActionResult Error(string error)
        {
            return View("Error", error);
        }
    }
}