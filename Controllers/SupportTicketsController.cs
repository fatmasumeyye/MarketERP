using MarketERP.Data;
using MarketERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MarketERP.Controllers
{
    public class SupportTicketsController : Controller
    {
        private readonly AppDbContext _context;

        public SupportTicketsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Customers = new SelectList(_context.Customers.ToList(), "Id", "FullName");

            var tickets = _context.SupportTickets
                .Include(t => t.Customer)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            return View(tickets);
        }

        [HttpPost]
        public IActionResult Add(SupportTicket ticket)
        {
            ticket.CreatedAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(ticket.Status))
            {
                ticket.Status = "Açık";
            }

            _context.SupportTickets.Add(ticket);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Resolve(int id)
        {
            var ticket = _context.SupportTickets.Find(id);

            if (ticket != null)
            {
                ticket.Status = "Çözüldü";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var ticket = _context.SupportTickets.Find(id);

            if (ticket != null)
            {
                _context.SupportTickets.Remove(ticket);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}