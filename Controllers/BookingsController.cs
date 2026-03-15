using Microsoft.AspNetCore.Mvc;
using EventEase_st10157545_POE.Data; //This is calling the folder in the solution 
using EventEase_st10157545_POE.Services;
using EventEase_st10157545_POE.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventEase_st10157545_POE.Models;
namespace EventEase_st10157545_POE.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseDbContext _context; //this is referencing the Database 
        private readonly BookingConflictService _conflictServices;
        private readonly AuditService _auditService; 

        public BookingsController(EventEaseDbContext context, BookingConflictService conflictServices, AuditService auditService)
        {
            _context = context;
            _conflictServices = conflictServices;
            _auditService = auditService;
        }


        //GET: Bookings
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
                b.Event!.EventName.Contains(vm.SearchTerm)
                );

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

        //GET: Bookings/Details/5
        public async Task<IActionResult> Details(int ? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .Include(b => b.Specialist)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if(booking == null) return NotFound();

            return View(booking);

        }

        //GET: Booking/Create
        public async Task<IActionResult> Create()
        {
            var vm = await BuildCreateVmAsync();
            return View(vm);
        }

        //POST: Bookings/Create
         [HttpPost, ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(BookingCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateVmAsync(vm);
                return View(vm);
            }

            var b = vm.Booking;
            // Conflict check
            if (await _conflictServices.HasConflictAsync(b.VenueID, b.EventDate, b.StartTime, b.EndTime))
            {
                var conflicts = await _conflictServices.GetConflictAsync(b.VenueID, b.EventDate, b.StartTime, b.EndTime);
                vm.ConflictMessage = $"This Venue is already booked on {b.EventDate:dd mm yyyy} " +
                    $"from {conflicts.First().StartTime:hh\\:mm} to {conflicts.First().EndTime:hh\\mm} " +
                    $"({conflicts.First().Customer?.FullName} - {conflicts.First().Event?.EventName}). " +
                    $"Please choose a different time or venue.";
                await PopulateCreateVmAsync(vm);
                return View(vm);

            }

            //Capacity check
            if(await _conflictServices.ExceedsCapacityAsync(b.VenueID, b.GuestCount))
            {
                var venue = await _context.Venue.FindAsync(b.VenueID);
                ModelState.AddModelError("Booking.GuestCount", $"Guest count exceeds venue capacity ({venue?.Capacity} max).");
                await PopulateCreateVmAsync(vm);
                return View(vm);
            }

            //Calculate total price

            var selectedVenue = await _context.Venue.FindAsync(b.VenueID);
            b.TotalPrice = selectedVenue?.PricePerDay ?? 0;
            b.BookingDate = DateTime.Now;

            _context.Bookings.Add(b);
            await _context.SaveChangesAsync();

            //Create schedule entry
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

            //Update event
            var ev = await _context.Event.FindAsync(b.EventID);
            if (ev != null)
            {
                ev.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }

            //Audit
            await _auditService.LogAsync(b.SpecialistID, "Create", "Booking", b.BookingID, $"Booking created for {b.EventDate:dd MM yyyy}");

            TempData["Success"] = $"Booking #{b.BookingID} created successfully.";
            return RedirectToAction(nameof(Details), new { id = b.BookingID });
        }
    }
}
