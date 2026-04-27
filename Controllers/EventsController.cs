using EventEase_st10157545_POE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Models;

namespace EventEase_st10157545_POE.Controllers
{
    public class EventsController : Controller
    {
            

        private readonly EventEaseDbContext _context;
        private readonly BlobStorageService _blob;
        public EventsController(EventEaseDbContext context, BlobStorageService blob)
        {
            _context = context;
            _blob = blob;
        }
        public async Task<IActionResult> Index(string? search, string? status, DateTime? fromDate, DateTime? toDate)
        {
            ViewData["Search"] = search;
            ViewData["Status"] = status;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;
            var query = _context.Event.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.EventName.Contains(search) || (e.Description != null && e.Description.Contains(search)));
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);
            if (fromDate.HasValue)
                query = query.Where(e => e.PreferredDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(e => e.PreferredDate <= toDate.Value);
            return View(await query.OrderBy(e => e.EventName).ToListAsync());
        }
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
        public IActionResult Create() => View();
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel ev)
        {
            ModelState.Remove("ImageFile");
            if (!ModelState.IsValid) return View(ev);
            if (ev.ImageFile != null && ev.ImageFile.Length > 0)
            {
                try { ev.ImageURL = await _blob.UploadEventImageAsync(ev.ImageFile); }
                catch (InvalidOperationException ex) { ModelState.AddModelError("ImageFile", ex.Message); return View(ev); }
            }
            _context.Event.Add(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Event '{ev.EventName}' created.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Event.FindAsync(id);
            if (ev == null) return NotFound();
            return View(ev);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel ev)
        {
            if (id != ev.EventID) return NotFound();
            ModelState.Remove("ImageFile");
            if (!ModelState.IsValid) return View(ev);
            if (ev.ImageFile != null && ev.ImageFile.Length > 0)
            {
                try
                {
                    var existing = await _context.Event.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == id);
                    if (existing?.ImageURL != null) await _blob.DeleteImageAsync(existing.ImageURL);
                    ev.ImageURL = await _blob.UploadEventImageAsync(ev.ImageFile);
                }
                catch (InvalidOperationException ex) { ModelState.AddModelError("ImageFile", ex.Message); return View(ev); }
            }
            _context.Update(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event updated.";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Event
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();
            // Enhancement 1: Guard active bookings
            var activeCount = ev.Bookings.Count(b => b.Status != "Cancelled");
            if (activeCount > 0)
                ViewData["ActiveBookingWarning"] = $"This event has {activeCount} active booking(s). You must cancel all bookings before deleting this event.";
            return View(ev);
        }
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Event.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();
            // Hard block: cannot delete with active bookings
            if (ev.Bookings.Any(b => b.Status != "Cancelled"))
            {
                TempData["Error"] = "Cannot delete this event. Cancel all associated bookings first.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            if (!string.IsNullOrEmpty(ev.ImageURL)) await _blob.DeleteImageAsync(ev.ImageURL);
            _context.Event.Remove(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event deleted.";
            return RedirectToAction(nameof(Index));
        }
}
