using EventEase_st10157545_POE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Models;

namespace EventEase_st10157545_POE.Controllers
{
    public class CustomersController : Controller
    {
        private readonly EventEaseDbContext _context;
        public CustomersController(EventEaseDbContext context) => _context = context;
        // GET: Customers
        public async Task<IActionResult> Index(string? search)
        {
            ViewData["Search"] = search;
            var query = _context.Customer.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c =>
                    c.FirstName.Contains(search) ||
                    c.LastName.Contains(search) ||
                    c.Email.Contains(search) ||
                    c.phoneNumber.Contains(search));
            return View(await query.OrderBy(c => c.LastName).ToListAsync());
        }
        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customer
                .Include(c => c.Bookings).ThenInclude(b => b.Venue)
                .Include(c => c.Bookings).ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer == null) return NotFound();
            return View(customer);
        }
        // GET: Customers/Create
        public IActionResult Create() => View();
        // POST: Customers/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerViewModel customer)
        {
            // Duplicate email check
            if (await _context.Customer.AnyAsync(c => c.Email == customer.Email))
            {
                ModelState.AddModelError("Email", "A customer with this email already exists.");
                return View(customer);
            }
            if (ModelState.IsValid)
            {
                customer.CreateAt = DateTime.Now;
                _context.Customer.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Customer '{customer.FullName}' added.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customer.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }
        // POST: Customers/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerViewModel customer)
        {
            if (id != customer.CustomerID) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }
        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer == null) return NotFound();
            return View(customer);
        }
        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer != null) { _context.Customer.Remove(customer); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Customer deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
