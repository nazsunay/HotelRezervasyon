using Microsoft.AspNetCore.Mvc;
using System;
using WebApplication3.Data;

namespace WebApplication3.Controllers
{
    public class LoginController : Controller
    {
        

        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Index()
        {
            return View();
        }

     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string username, string password)
        {
           
            var user = _context.Logins
                .FirstOrDefault(u => u.UserName == username && u.Password == password);

            if (user != null)
            {
             
                HttpContext.Session.SetString("IsAuthenticated", "true");
                HttpContext.Session.SetString("Username", user.UserName);

                
                return RedirectToAction("Index", "Admin");
            }

            
            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre!";
            return View();
        }

        public IActionResult Logout()
        {
            
            HttpContext.Session.Remove("IsAuthenticated");
            HttpContext.Session.Remove("Username");

            
            return RedirectToAction("Index", "Home");
        }
    }
}
