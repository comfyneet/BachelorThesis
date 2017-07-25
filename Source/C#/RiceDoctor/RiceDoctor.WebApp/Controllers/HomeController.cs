using System.Linq;
using JetBrains.Annotations;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using RiceDoctor.DatabaseManager;
using RiceDoctor.Shared;
using RiceDoctor.WebApp.Models;

namespace RiceDoctor.WebApp.Controllers
{
    public class HomeController : Controller
    {
        [NotNull] private static readonly FeedbackAccount Account;
        [NotNull] private readonly RiceContext _context;

        static HomeController()
        {
            Account = new FeedbackAccount
            {
                Email = "ricedoctor.uit@gmail.com",
                Password = "uit.edu.vn",
                Host = "smtp.gmail.com",
                Port = 587
            };
        }

        public HomeController([NotNull] RiceContext context)
        {
            Check.NotNull(context, nameof(context));

            _context = context;
        }

        public IActionResult Index()
        {
            var articles = _context.Articles
                .Include(nameof(Website))
                .OrderByDescending(a => a.RetrievedDate)
                .Take(5)
                .ToList();

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
                message.From.Add(new MailboxAddress("Feedback", Account.Email));
                message.To.Add(new MailboxAddress("", Account.Email));
                message.Subject = $"{feedback.Subject.Trim()} from {feedback.Name.Trim()} ({feedback.Email.Trim()})";
                message.Body = new TextPart("plain") {Text = feedback.Message.Trim()};

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(Account.Host, Account.Port);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(Account.Email, Account.Password);
                    client.Send(message);
                    client.Disconnect(true);
                }

                ViewData["SentMail"] = Account.Email;
            }

            return View("Contact");
        }

        public IActionResult Error(string error)
        {
            return View("Error", error);
        }
    }
}