using EventEase_st10157545_POE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase_st10157545_POE.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly EventEaseDbContext _context;
        public ScheduleController(EventEaseDbContext context) => _context = context;
        // GET: Schedule  — monthly calendar view
        public async Task<IActionResult> Index(int? year, int? month, int? venueId)
        {
            var now = DateTime.Now;
            var y = year ?? now.Year;
            var m = month ?? now.Month;
            var firstDay = new DateTime(y, m, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            ViewData["Year"] = y;
            ViewData["Month"] = m;
            ViewData["MonthName"] = firstDay.ToString("MMMM yyyy");
            ViewData["Venues"] = await _context.Venue.Where(v => v.IsActive).OrderBy(v => v.VenueName).ToListAsync();
            ViewData["SelectedVenue"] = venueId;
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Where(b => b.EventDate >= firstDay && b.EventDate <= lastDay && b.Status != "Cancelled");
            if (venueId.HasValue)
                query = query.Where(b => b.VenueID == venueId.Value);
            var bookings = await query.OrderBy(b => b.EventDate).ThenBy(b => b.StartTime).ToListAsync();
            return View(bookings);
        }
        // GET: Schedule/Day?date=2026-06-15
        public async Task<IActionResult> Day(DateTime? date, int? venueId)
        {
            var d = date ?? DateTime.Today;
            ViewData["Date"] = d;
            ViewData["Venues"] = await _context.Venue.Where(v => v.IsActive).OrderBy(v => v.VenueName).ToListAsync();
            ViewData["SelectedVenue"] = venueId;
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Where(b => b.EventDate.Date == d.Date && b.Status != "Cancelled");
            if (venueId.HasValue)
                query = query.Where(b => b.VenueID == venueId.Value);
            return View(await query.OrderBy(b => b.StartTime).ToListAsync());
        }
        // GET: AJAX — returns bookings as JSON for calendar rendering
        [HttpGet]
        public async Task<IActionResult> Events(int? venueId, string? start, string? end)
        {
            DateTime.TryParse(start, out var startDate);
            DateTime.TryParse(end, out var endDate);
            var query = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Include(b => b.Customer)
                .Where(b => b.Status != "Cancelled");
            if (venueId.HasValue) query = query.Where(b => b.VenueID == venueId.Value);
            if (startDate != default) query = query.Where(b => b.EventDate >= startDate);
            if (endDate != default) query = query.Where(b => b.EventDate <= endDate);
            var bookings = await query.ToListAsync();
            var events = bookings.Select(b => new
            {
                id = b.BookingID,
                title = $"{b.Event?.EventName} — {b.Customer?.FullName}",
                start = b.EventDate.ToString("yyyy-MM-dd") + "T" + b.StartTime.ToString(@"hh\:mm"),
                end = b.EventDate.ToString("yyyy-MM-dd") + "T" + b.EndTime.ToString(@"hh\:mm"),
                color = b.Status == "Confirmed" ? "#1d9e75" : "#ba7517",
                extendedProps = new { venue = b.Venue?.VenueName, status = b.Status, bookingId = b.BookingID }
            });
            return Json(events);
        }
    }
}
