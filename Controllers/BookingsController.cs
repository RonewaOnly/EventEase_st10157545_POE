using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Models.ViewModels;
using EventEase_st10157545_POE.Models;

using EventEase_st10157545_POE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseDbContext _context;
        private readonly BookingConflictService _conflictService;
        private readonly AuditService _auditService;

        public BookingsController(
            EventEaseDbContext context,
            BookingConflictService conflictService,
            AuditService auditService)
        {
            _context = context;
            _conflictService = conflictService;
            _auditService = auditService;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(BookingSearchViewModel vm)
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Include(b => b.Specialist)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(vm.SearchTerm))
                query = query.Where(b =>
                    b.Customer!.FirstName.Contains(vm.SearchTerm) ||
                    b.Customer!.LastName.Contains(vm.SearchTerm) ||
                    b.Venue!.VenueName.Contains(vm.SearchTerm) ||
                    b.Event!.EventName.Contains(vm.SearchTerm));

            if (!string.IsNullOrWhiteSpace(vm.StatusFilter))
                query = query.Where(b => b.Status == vm.StatusFilter);

            if (vm.FromDate.HasValue)
                query = query.Where(b => b.EventDate >= vm.FromDate.Value);

            if (vm.ToDate.HasValue)
                query = query.Where(b => b.EventDate <= vm.ToDate.Value);

            if (vm.VenueFilter.HasValue)
                query = query.Where(b => b.VenueID == vm.VenueFilter.Value);

            vm.Bookings = await query.OrderByDescending(b => b.EventDate).ToListAsync();
            vm.Venues = new SelectList(await _context.Venue.Where(v => v.IsActive).ToListAsync(), "VenueID", "VenueName");

            return View(vm);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Include(b => b.Specialist)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound();
            return View(booking);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create()
        {
            var vm = await BuildCreateVmAsync();
            return View(vm);
        }

        // POST: Bookings/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateVmAsync(vm);
                return View(vm);
            }

            var b = vm.Booking;

            // ── Conflict check ────────────────────────────────────────────────
            if (await _conflictService.HasConflictAsync(b.VenueID, b.EventDate, b.StartTime, b.EndTime))
            {
                var conflicts = await _conflictService.GetConflictAsync(b.VenueID, b.EventDate, b.StartTime, b.EndTime);
                vm.ConflictMessage = $"This venue is already booked on {b.EventDate:dd MMM yyyy} " +
                    $"from {conflicts.First().StartTime:hh\\:mm} to {conflicts.First().EndTime:hh\\:mm} " +
                    $"({conflicts.First().Customer?.FullName} — {conflicts.First().Event?.EventName}). " +
                    "Please choose a different time or venue.";
                await PopulateCreateVmAsync(vm);
                return View(vm);
            }

            // ── Capacity check 
            if (await _conflictService.ExceedsCapacityAsync(b.VenueID, b.GuestCount))
            {
                var venue = await _context.Venue.FindAsync(b.VenueID);
                ModelState.AddModelError("Booking.GuestCount",
                    $"Guest count exceeds venue capacity ({venue?.Capacity} max).");
                await PopulateCreateVmAsync(vm);
                return View(vm);
            }

            // ── Calculate total price 
            var selectedVenue = await _context.Venue.FindAsync(b.VenueID);
            b.TotalPrice = selectedVenue?.PricePerDay ?? 0;
            b.BookingDate = DateTime.Now;

            _context.Bookings.Add(b);
            await _context.SaveChangesAsync();

            // ── Create schedule entry 
            _context.VenueSchedules.Add(new VenueAvaliabilityViewModel
            {
                VenueID = b.VenueID,
                BookingID = b.BookingID,
                EventDate = b.EventDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = "Reserved"
            });
            await _context.SaveChangesAsync();

            // ── Update event status 
            var ev = await _context.Event.FindAsync(b.EventID);
            if (ev != null)
            {
                ev.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }

            // ── Audit 
            await _auditService.LogAsync(b.SpecialistID, "Create", "Booking", b.BookingID,
                $"Booking created for {b.EventDate:dd MMM yyyy}");

            TempData["Success"] = $"Booking #{b.BookingID} created successfully.";
            return RedirectToAction(nameof(Details), new { id = b.BookingID });
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            var vm = await BuildCreateVmAsync();
            vm.Booking = booking;
            return View(vm);
        }

        // POST: Bookings/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookingCreateViewModel vm)
        {
            if (id != vm.Booking.BookingID) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateCreateVmAsync(vm);
                return View(vm);
            }

            var b = vm.Booking;

            // Conflict check (exclude this booking from the check)
            if (await _conflictService.HasConflictAsync(b.VenueID, b.EventDate, b.StartTime, b.EndTime, b.BookingID))
            {
                var conflicts = await _conflictService.GetConflictAsync(b.VenueID, b.EventDate, b.StartTime, b.EndTime, b.BookingID);
                vm.ConflictMessage = $"Venue conflict: {conflicts.First().Customer?.FullName} — {conflicts.First().Event?.EventName} " +
                    $"({conflicts.First().StartTime:hh\\:mm}–{conflicts.First().EndTime:hh\\:mm}).";
                await PopulateCreateVmAsync(vm);
                return View(vm);
            }

            try
            {
                // Recalculate price
                var venue = await _context.Venue.FindAsync(b.VenueID);
                b.TotalPrice = venue?.PricePerDay ?? 0;

                _context.Update(b);

                // Update schedule entry
                var schedule = await _context.VenueSchedules.FirstOrDefaultAsync(vs => vs.BookingID == b.BookingID);
                if (schedule != null)
                {
                    schedule.VenueID = b.VenueID;
                    schedule.EventDate = b.EventDate;
                    schedule.StartTime = b.StartTime;
                    schedule.EndTime = b.EndTime;
                }

                await _context.SaveChangesAsync();
                await _auditService.LogAsync(b.SpecialistID, "Update", "Booking", b.BookingID, "Booking updated");

                TempData["Success"] = "Booking updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Bookings.Any(b2 => b2.BookingID == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Details), new { id = b.BookingID });
        }

        // POST: Bookings/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            booking.Status = "Cancelled";

            // Free up the schedule slot
            var schedule = await _context.VenueSchedules.FirstOrDefaultAsync(vs => vs.BookingID == id);
            if (schedule != null) schedule.Status = "Available";

            // Revert event to Pending
            var ev = await _context.Event.FindAsync(booking.EventID);
            if (ev != null) ev.Status = "Pending";

            await _context.SaveChangesAsync();
            await _auditService.LogAsync(booking.SpecialistID, "Cancel", "Booking", id, "Booking cancelled");

            TempData["Warning"] = $"Booking #{id} has been cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Customer).Include(b => b.Venue).Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.BookingID == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                // Remove schedule entry first
                var schedule = await _context.VenueSchedules.FirstOrDefaultAsync(vs => vs.BookingID == id);
                if (schedule != null) _context.VenueSchedules.Remove(schedule);

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ── AJAX: check availability before submitting form 
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int venueId, string eventDate, string startTime, string endTime, int? excludeId)
        {
            if (!DateTime.TryParse(eventDate, out var date) ||
                !TimeSpan.TryParse(startTime, out var start) ||
                !TimeSpan.TryParse(endTime, out var end))
                return Json(new { available = false, message = "Invalid date/time format." });

            var hasConflict = await _conflictService.HasConflictAsync(venueId, date, start, end, excludeId);
            return Json(new
            {
                available = !hasConflict,
                message = hasConflict ? "This slot is already booked." : "Slot is available!"
            });
        }

        // ── Helpers 
        private async Task<BookingCreateViewModel> BuildCreateVmAsync()
        {
            var vm = new BookingCreateViewModel();
            await PopulateCreateVmAsync(vm);
            return vm;
        }

        private async Task PopulateCreateVmAsync(BookingCreateViewModel vm)
        {
            vm.Customers = new SelectList(

                await _context.Customer.OrderBy(c => c.LastName).ToListAsync(),
                "CustomerID", "FullName");

            vm.Venues = new SelectList(
                await _context.Venue.Where(v => v.IsActive).OrderBy(v => v.VenueName).ToListAsync(),
                "VenueID", "VenueName");

            vm.Events = new SelectList(
                await _context.Event.OrderBy(e => e.EventName).ToListAsync(),
                "EventID", "EventName");

            vm.Specialists = new SelectList(
                await _context.BookingSpecialist.OrderBy(s => s.LastName).ToListAsync(),
                "SpecialistID", "FullName");
        }
    }
}
