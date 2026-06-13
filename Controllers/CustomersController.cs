using System.Globalization;
using MarketERP.Data;
using MarketERP.Helpers;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarketERP.Controllers
{
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var customers = _context.Customers
                .OrderBy(c => c.FullName)
                .ToList();

            return View(customers);
        }

        [HttpPost]
        public IActionResult Add(Customer customer)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var canManageDiscount =
                HttpContext.HasPermission("sale.wholesale.create")
                || HttpContext.HasPermission("role.manage")
                || HttpContext.HasPermission("user.manage")
                || HttpContext.HasPermission("sale.view.all");

            if (canManageDiscount)
            {
                var discountRateText = Request.Form["DiscountRate"].ToString();

                if (!string.IsNullOrWhiteSpace(discountRateText))
                {
                    discountRateText = discountRateText.Replace(",", ".");

                    if (decimal.TryParse(
                            discountRateText,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out decimal parsedDiscountRate))
                    {
                        customer.DiscountRate = parsedDiscountRate;
                    }
                    else
                    {
                        customer.DiscountRate = 0;
                    }
                }
                else
                {
                    customer.DiscountRate = 0;
                }
            }
            else
            {
                customer.DiscountRate = 0;
            }

            if (customer.DiscountRate < 0)
            {
                customer.DiscountRate = 0;
            }

            if (customer.DiscountRate > 100)
            {
                customer.DiscountRate = 100;
            }

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateDiscount(int id, string discountRate)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var canManageDiscount =
                HttpContext.HasPermission("sale.wholesale.create")
                || HttpContext.HasPermission("role.manage")
                || HttpContext.HasPermission("user.manage")
                || HttpContext.HasPermission("sale.view.all");

            if (!canManageDiscount)
            {
                return RedirectToAction("Index");
            }

            var customer = _context.Customers.Find(id);

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            decimal parsedDiscountRate = 0;

            if (!string.IsNullOrWhiteSpace(discountRate))
            {
                discountRate = discountRate.Replace(",", ".");

                decimal.TryParse(
                    discountRate,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out parsedDiscountRate);
            }

            if (parsedDiscountRate < 0)
            {
                parsedDiscountRate = 0;
            }

            if (parsedDiscountRate > 100)
            {
                parsedDiscountRate = 100;
            }

            customer.DiscountRate = parsedDiscountRate;

            _context.Customers.Update(customer);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var customer = _context.Customers.Find(id);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult DiscountedCustomers()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var customers = _context.Customers
                .Where(c => c.DiscountRate > 0)
                .OrderByDescending(c => c.DiscountRate)
                .ThenBy(c => c.FullName)
                .ToList();

            return View(customers);
        }
    }
}