using EventEase_st10157545_POE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Models;

namespace EventEase_st10157545_POE.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEaseDbContext _context;
        public EventsController(EventEaseDbContext context) => _context = context;
        // GET: Events
        public async Task<IActionResult> Index(string? search, string? status)
        {
            ViewData["Search"] = search;
            ViewData["Status"] = status;
            var query = _context.Event.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.EventName.Contains(search) || (e.Description != null && e.Description.Contains(search)));
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);
            return View(await query.OrderBy(e => e.EventName).ToListAsync());
        }
        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Event
                .Include(e => e.Bookings).ThenInclude(b => b.Venue)
                .Include(e => e.Bookings).ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();
            return View(ev);
        }
        // GET: Events/Create
        public IActionResult Create() => View();
        // POST: Events/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel ev)
        {
            if (ModelState.IsValid)
            {
                _context.Event.Add(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Event '{ev.EventName}' created. You can now create a booking for it once a venue is ready.";
                return RedirectToAction(nameof(Index));
            }
            return View(ev);
        }
        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Event.FindAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }
        // POST: Events/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel ev)
        {
            if (id != ev.EventID) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(ev);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(ev);
        }
        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Event.FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();
            return View(ev);
        }
        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Event.FindAsync(id);
            if (ev != null) { _context.Event.Remove(ev); await _context.SaveChangesAsync(); }
            TempData["Success"] = "Event deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
