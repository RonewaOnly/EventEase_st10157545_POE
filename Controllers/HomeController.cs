using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Models;
using EventEase_st10157545_POE.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EventEase_st10157545_POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EventEaseDbContext _context;

        public HomeController(ILogger<HomeController> logger, EventEaseDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var vm = new DashboardViewModel
            {
                TotalBookings = await _context.Bookings.CountAsync(),
                PendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Pending"),
                ConfirmedBooking = await _context.Bookings.CountAsync(b => b.Status == "Confirmed"),
                TotalVenues = await _context.Venue.CountAsync(v => v.IsActive),
                TotalCustomers = await _context.Customer.CountAsync(),
                PendingEvents = await _context.Event.CountAsync(e => e.Status == "Pending"),
                RevenueThisMonth = await _context.Bookings
                    .Where(b => b.Status == "Confirmed"
                             && b.BookingDate.Month == now.Month
                             && b.BookingDate.Year == now.Year)
                    .SumAsync(b => (decimal?)b.TotalPrice) ?? 0,
                UpcomingBookings = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Venue)
                    .Include(b => b.Event)
                    .Where(b => b.EventDate >= now.Date && b.Status != "Cancelled")
                    .OrderBy(b => b.EventDate)
                    .Take(5)
                    .ToListAsync(),
                RecentBookings = await _context.Bookings
                    .Include(b => b.Customer)
                    .Include(b => b.Venue)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .ToListAsync()
            };
            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
