using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_st10157545_POE.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEaseDbContext _context;

        public VenuesController(EventEaseDbContext context)
        {
            _context = context;
        }

        // GET: Venues
        public async Task<IActionResult> Index(string? search, bool showInactive = false)
        {
            ViewData["Search"] = search;
            ViewData["ShowInactive"] = showInactive;

            var query = _context.Venue.AsQueryable();

            if (!showInactive)
                query = query.Where(v => v.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(v =>
                    v.VenueName.Contains(search) ||
                    v.Location.Contains(search));

            return View(await query.OrderBy(v => v.VenueName).ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venue
                .Include(v => v.Bookings)
                    .ThenInclude(b => b.Customer)
                .Include(v => v.Bookings)
                    .ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(v => v.VenueID == id);

            if (venue == null) return NotFound();
            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create() => View();

        // POST: Venues/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VenueViewModel venue)
        {
            if (ModelState.IsValid)
            {
                _context.Venue.Add(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Venue '{venue.VenueName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VenueViewModel venue)
        {
            if (id != venue.VenueID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Venue updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venue.Any(v => v.VenueID == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venue.FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue != null)
            {
                // Soft delete — mark inactive instead of removing
                venue.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue deactivated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Venues/Availability/5?date=2026-06-15
        public async Task<IActionResult> Availability(int id, DateTime? date)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();

            var checkDate = date ?? DateTime.Today;

            var schedules = await _context.VenueSchedules
                .Include(vs => vs.Booking)
                    .ThenInclude(b => b!.Customer)
                .Where(vs => vs.VenueID == id && vs.EventDate.Value == checkDate.Date)
                .OrderBy(vs => vs.StartTime)
                .ToListAsync();

            ViewData["Venue"] = venue;
            ViewData["Date"] = checkDate;
            return View(schedules);
        }
    }
}
