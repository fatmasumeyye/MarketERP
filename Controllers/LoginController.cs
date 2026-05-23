using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            var user = _context.Employees
                .Include(e => e.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefault(e =>
                    e.Username == username &&
                    e.IsActive);

            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }

            var passwordHasher = new PasswordHasher<Employee>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }

            HttpContext.Session.SetInt32("EmployeeId", user.Id);
            HttpContext.Session.SetString("Username", user.Username ?? "");
            HttpContext.Session.SetString("FullName", user.FullName);

            var roleNames = user.UserRoles?
                .Select(ur => ur.Role.Name)
                .ToList();

            if (roleNames != null && roleNames.Any())
            {
                HttpContext.Session.SetString("Roles", string.Join(",", roleNames));
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}