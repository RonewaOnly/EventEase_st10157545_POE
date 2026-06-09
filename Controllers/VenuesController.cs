
using EventEase_st10157545_POE.Models;
using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Filter;
using EventEase_st10157545_POE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEase_st10157545_POE.Models.ViewModels;

namespace EventEase.Controllers
{
    [RequireLogin]
    public class VenuesController : Controller
    {
        private readonly EventEaseDbContext _context;
        private readonly BlobStorageService _blob;

        public VenuesController(EventEaseDbContext context, BlobStorageService blob)
        {
            _context = context;
            _blob = blob;
        }

        // GET: Venues — Part 3 enhanced filters incl. availability
        public async Task<IActionResult> Index(VenueSearchViewModel vm)
        {
            var query = _context.Venue.AsQueryable();

            if (!vm.ShowInactive)
                query = query.Where(v => v.IsActive);

            if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
                query = query.Where(v =>
                    v.VenueName.Contains(vm.SearchTerm) ||
                    v.Location.Contains(vm.SearchTerm) ||
                    (v.Description != null && v.Description.Contains(vm.SearchTerm)));

            if (vm.MinCapacity.HasValue)
                query = query.Where(v => v.Capacity >= vm.MinCapacity.Value);

            if (vm.MaxCapacity.HasValue)
                query = query.Where(v => v.Capacity <= vm.MaxCapacity.Value);

            if (vm.MinPrice.HasValue)
                query = query.Where(v => v.PricePerDay >= vm.MinPrice.Value);

            if (vm.MaxPrice.HasValue)
                query = query.Where(v => v.PricePerDay <= vm.MaxPrice.Value);

            // Part 3: availability filter
            if (vm.AvailableOnly)
                query = query.Where(v => v.IsAvailable);

            vm.Venues = await query.OrderBy(v => v.VenueName).ToListAsync();
            return View(vm);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venue
                .Include(v => v.Bookings).ThenInclude(b => b.Customer)
                .Include(v => v.Bookings).ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        public IActionResult Create() => View(new VenueViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VenueViewModel venue)
        {
            ModelState.Remove("ImageFile");
            if (!ModelState.IsValid) return View(venue);

            if (venue.ImageFile != null && venue.ImageFile.Length > 0)
            {
                try { venue.ImageUrl = await _blob.UploadVenueImageAsync(venue.ImageFile); }
                catch (InvalidOperationException ex)
                { ModelState.AddModelError("ImageFile", ex.Message); return View(venue); }
            }

            _context.Venue.Add(venue);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Venue '{venue.VenueName}' created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VenueViewModel venue)
        {
            if (id != venue.VenueID) return NotFound();
            ModelState.Remove("ImageFile");
            if (!ModelState.IsValid) return View(venue);

            try
            {
                if (venue.ImageFile != null && venue.ImageFile.Length > 0)
                {
                    var old = await _context.Venue.AsNoTracking().FirstOrDefaultAsync(v => v.VenueID == id);
                    if (old?.ImageUrl != null) await _blob.DeleteImageAsync(old.ImageUrl);
                    try { venue.ImageUrl = await _blob.UploadVenueImageAsync(venue.ImageFile); }
                    catch (InvalidOperationException ex)
                    { ModelState.AddModelError("ImageFile", ex.Message); return View(venue); }
                }

                _context.Update(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue updated.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Venue.Any(v => v.VenueID == id)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venue.Include(v => v.Bookings).FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();

            var activeCount = venue.Bookings.Count(b => b.Status != "Cancelled");
            if (activeCount > 0)
                ViewData["ActiveBookingWarning"] =
                    $"This venue has {activeCount} active booking(s). " +
                    "It will be deactivated rather than permanently deleted.";
            return View(venue);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue.Include(v => v.Bookings).FirstOrDefaultAsync(v => v.VenueID == id);
            if (venue == null) return NotFound();

            if (venue.Bookings.Any(b => b.Status != "Cancelled"))
            {
                venue.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Warning"] = "Venue deactivated — active bookings exist.";
            }
            else
            {
                if (!string.IsNullOrEmpty(venue.ImageUrl)) await _blob.DeleteImageAsync(venue.ImageUrl);
                _context.Venue.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Venue deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Availability(int id, DateTime? date)
        {
            var venue = await _context.Venue.FindAsync(id);
            if (venue == null) return NotFound();
            var checkDate = date ?? DateTime.Today;
            var schedules = await _context.VenueSchedules
                .Include(vs => vs.Booking).ThenInclude(b => b!.Customer)
                .Where(vs => vs.VenueID == id && vs.EventDate == checkDate.Date)
                .OrderBy(vs => vs.StartTime).ToListAsync();
            ViewData["Venue"] = venue;
            ViewData["Date"] = checkDate;
            return View(schedules);
        }
    }
}
