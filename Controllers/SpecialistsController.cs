using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_st10157545_POE.Controllers
{
    public class SpecialistsController : Controller
    {
        private readonly EventEaseDbContext _context;
        public SpecialistsController(EventEaseDbContext context) => _context = context;
        // GET: Specialists
        public async Task<IActionResult> Index()
            => View(await _context.BookingSpecialist.OrderBy(s => s.LastName).ToListAsync());
        // GET: Specialists/Create
        public IActionResult Create() => View();
        // POST: Specialists/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingSpecialistViewModelcs specialist, string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters.");
                return View(specialist);
            }
            if (await _context.BookingSpecialist.AnyAsync(s => s.Email == specialist.Email))
            {
                ModelState.AddModelError("Email", "A specialist with this email already exists.");
                return View(specialist);
            }
            if (ModelState.IsValid)
            {
                specialist.Password = BCrypt.Net.BCrypt.HashPassword(password);
                _context.BookingSpecialist.Add(specialist);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Specialist '{specialist.FullName}' created.";
                return RedirectToAction(nameof(Index));
            }
            return View(specialist);
        }
        // GET: Specialists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var s = await _context.BookingSpecialist.FindAsync(id);
            if (s == null) return NotFound();
            return View(s);
        }
        // POST: Specialists/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingSpecialistViewModelcs specialist, string? newPassword)
        {
            if (id != specialist.SpecialistID) return NotFound();
            var existing = await _context.BookingSpecialist.FindAsync(id);
            if (existing == null) return NotFound();
            existing.FirstName = specialist.FirstName;
            existing.LastName = specialist.LastName;
            existing.Email = specialist.Email;
            existing.Role = specialist.Role;
            if (!string.IsNullOrWhiteSpace(newPassword))
                existing.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Specialist updated.";
            return RedirectToAction(nameof(Index));
        }
        // POST: Specialists/Delete/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _context.BookingSpecialist.FindAsync(id);
            if (s != null) { _context.BookingSpecialist.Remove(s); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Specialist removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
