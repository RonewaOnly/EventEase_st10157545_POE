
using EventEase_st10157545_POE.Models;
using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Filter;
using EventEase_st10157545_POE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Models.ViewModels;

namespace EventEase.Controllers
{
    [RequireLogin]
    public class EventsController : Controller
    {
        private readonly EventEaseDbContext _context;
        private readonly BlobStorageService _blob;

        public EventsController(EventEaseDbContext context, BlobStorageService blob)
        {
            _context = context;
            _blob = blob;
        }

        public async Task<IActionResult> Index(EventSearchViewModel vm)
        {
            var query = _context.Event.Include(e => e.EventType).AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
                query = query.Where(e => e.EventName.Contains(vm.SearchTerm) ||
                    (e.Description != null && e.Description.Contains(vm.SearchTerm)));

            if (!string.IsNullOrWhiteSpace(vm.StatusFilter))
                query = query.Where(e => e.Status == vm.StatusFilter);

            // Part 3: filter by event type
            if (vm.EventTypeFilter.HasValue)
                query = query.Where(e => e.EventTypeID == vm.EventTypeFilter.Value);

            if (vm.FromDate.HasValue)
                query = query.Where(e => e.PreferredDate >= vm.FromDate.Value);

            if (vm.ToDate.HasValue)
                query = query.Where(e => e.PreferredDate <= vm.ToDate.Value);

            vm.Events = await query.OrderBy(e => e.EventName).ToListAsync();
            vm.EventTypes = new SelectList(
                await _context.EventTypes.Where(t => t.IsActive).OrderBy(t => t.TypeName).ToListAsync(),
                "EventTypeID", "TypeName", vm.EventTypeFilter);

            return View(vm);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Event
                .Include(e => e.EventType)
                .Include(e => e.Bookings).ThenInclude(b => b.Venue)
                .Include(e => e.Bookings).ThenInclude(b => b.Customer)
                .FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();
            return View(ev);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateEventTypes();
            return View(new EventViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel ev)
        {
            ModelState.Remove("ImageFile");
            ModelState.Remove("EventType");
            if (!ModelState.IsValid) { await PopulateEventTypes(); return View(ev); }

            if (ev.ImageFile != null && ev.ImageFile.Length > 0)
            {
                try { ev.ImageURL = await _blob.UploadEventImageAsync(ev.ImageFile); }
                catch (InvalidOperationException ex)
                { ModelState.AddModelError("ImageFile", ex.Message); await PopulateEventTypes(); return View(ev); }
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
            await PopulateEventTypes(ev.EventTypeID);
            return View(ev);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel ev)
        {
            if (id != ev.EventID) return NotFound();
            ModelState.Remove("ImageFile");
            ModelState.Remove("EventType");
            if (!ModelState.IsValid) { await PopulateEventTypes(ev.EventTypeID); return View(ev); }

            if (ev.ImageFile != null && ev.ImageFile.Length > 0)
            {
                var old = await _context.Event.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == id);
                if (old?.ImageURL != null) await _blob.DeleteImageAsync(old.ImageURL);
                try { ev.ImageURL = await _blob.UploadEventImageAsync(ev.ImageFile); }
                catch (InvalidOperationException ex)
                { ModelState.AddModelError("ImageFile", ex.Message); await PopulateEventTypes(ev.EventTypeID); return View(ev); }
            }

            _context.Update(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event updated.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ev = await _context.Event.Include(e => e.EventType).Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();

            var activeCount = ev.Bookings.Count(b => b.Status != "Cancelled");
            if (activeCount > 0)
                ViewData["ActiveBookingWarning"] =
                    $"This event has {activeCount} active booking(s). Cancel all bookings first.";
            return View(ev);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Event.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.EventID == id);
            if (ev == null) return NotFound();

            if (ev.Bookings.Any(b => b.Status != "Cancelled"))
            {
                TempData["Error"] = "Cannot delete — cancel all associated bookings first.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            if (!string.IsNullOrEmpty(ev.ImageURL)) await _blob.DeleteImageAsync(ev.ImageURL);
            _context.Event.Remove(ev);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Event deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ── Event Type management (Admin only) ────────────────────────────────
        [RequireAdmin]
        public async Task<IActionResult> EventTypes()
            => View(await _context.EventTypes.OrderBy(t => t.TypeName).ToListAsync());

        [RequireAdmin]
        public IActionResult CreateEventType() => View(new EventType());

        [HttpPost, ValidateAntiForgeryToken, RequireAdmin]
        public async Task<IActionResult> CreateEventType(EventType et)
        {
            if (!ModelState.IsValid) return View(et);
            _context.EventTypes.Add(et);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Event type '{et.TypeName}' added.";
            return RedirectToAction(nameof(EventTypes));
        }

        [HttpPost, ValidateAntiForgeryToken, RequireAdmin]
        public async Task<IActionResult> ToggleEventType(int id)
        {
            var et = await _context.EventTypes.FindAsync(id);
            if (et == null) return NotFound();
            et.IsActive = !et.IsActive;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Event type '{et.TypeName}' {(et.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(EventTypes));
        }

        // Helper
        private async Task PopulateEventTypes(int? selected = null)
        {
            ViewBag.EventTypes = new SelectList(
                await _context.EventTypes.Where(t => t.IsActive).OrderBy(t => t.TypeName).ToListAsync(),
                "EventTypeID", "TypeName", selected);
        }
    }
}
