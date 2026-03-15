using EventEase_st10157545_POE.Data;
using EventEase_st10157545_POE.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase_st10157545_POE.Services
{
    public class BookingConflictService
    {
        private readonly EventEaseDbContext _context;

        public BookingConflictService(EventEaseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasConflictAsync(
            int venueid,
            DateTime eventDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludeBookingId = null)
        {
            var query = _context.Bookings
                .Where(b =>
                    b.VenueID == venueid &&
                    b.EventDate.Date == eventDate.Date &&
                    b.Status != "Cancelled" &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime
                );

            if(excludeBookingId.HasValue ) 
                query = query.Where(b => b.BookingID != excludeBookingId.Value);

            return await query.AnyAsync();
        }

        public async Task<List<Booking>> GetConflictAsync(
            int venueId,
            DateTime eventDate,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludeBookingId = null
            )
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Event)
                .Where(b =>
                    b.VenueID == venueId &&
                    b.EventDate.Date == eventDate.Date &&
                    b.Status != "Cancelled" &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime
                );

            if (excludeBookingId.HasValue)
                query = query.Where(b => b.BookingID != excludeBookingId.Value);

            return await  query.ToListAsync();
        }

        public async Task<bool> ExceedsCapacityAsync(int venueId, int guestCount)
        {
            var venue = await _context.Venue.FindAsync(venueId);
            return venue != null && guestCount > venue.Capacity;
        }


    }
}
