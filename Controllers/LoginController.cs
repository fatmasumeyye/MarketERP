using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string username, string password)
        {
            HttpContext.Session.Clear();

            var normalizedUsername = username?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedUsername) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre alanları zorunludur.";
                ViewBag.Username = normalizedUsername;
                return View();
            }

            var normalizedUsernameLower = normalizedUsername.ToLower();
            var user = await _context.Employees
                .FirstOrDefaultAsync(e =>
                    e.Username != null &&
                    e.Username.Trim().ToLower() == normalizedUsernameLower);

            if (user == null)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                ViewBag.Username = normalizedUsername;
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Bu kullanıcı hesabı aktif değil.";
                ViewBag.Username = normalizedUsername;
                return View();
            }

            if (string.IsNullOrEmpty(user.Password) ||
                !PasswordMatches(user, password))
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
                ViewBag.Username = normalizedUsername;
                return View();
            }

            HttpContext.Session.SetInt32("EmployeeId", user.Id);
            HttpContext.Session.SetString("Username", user.Username!.Trim());
            HttpContext.Session.SetString("FullName", user.FullName);

            var roleNames = await _context.UserRoles
                .Where(ur => ur.EmployeeId == user.Id)
                .Select(ur => ur.Role.Name)
                .Distinct()
                .ToListAsync();

            var permissionCodes = await _context.UserRoles
                .Where(ur => ur.EmployeeId == user.Id)
                .SelectMany(ur => ur.Role.RolePermissions!)
                .Select(rp => rp.Permission.Code)
                .Distinct()
                .ToListAsync();

            HttpContext.Session.SetString("Roles", string.Join(",", roleNames));
            HttpContext.Session.SetString(
                "Permissions",
                string.Join(",", permissionCodes));

            return RedirectToAction("Index", "Home");
        }

        private static bool PasswordMatches(Employee user, string suppliedPassword)
        {
            var storedPassword = user.Password!;

            // Eski kayıtlarda şifreler düz metin tutulmuş olabilir.
            if (FixedTimeEquals(storedPassword, suppliedPassword))
            {
                return true;
            }

            try
            {
                var passwordHasher = new PasswordHasher<Employee>();
                var result = passwordHasher.VerifyHashedPassword(
                    user,
                    storedPassword,
                    suppliedPassword);

                return result != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool FixedTimeEquals(string storedValue, string suppliedValue)
        {
            var storedBytes = Encoding.UTF8.GetBytes(storedValue);
            var suppliedBytes = Encoding.UTF8.GetBytes(suppliedValue);

            return storedBytes.Length == suppliedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(
                       storedBytes,
                       suppliedBytes);
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
