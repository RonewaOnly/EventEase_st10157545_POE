using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace EventEase_st10157545_POE.Models.ViewModels
{
    // Used on the creation Booking Form

    public class BookingCreateViewModel
    {
        public Booking Booking { get; set; } = new();

        public SelectList? Customers { get; set; } 

        public SelectList? Venues { get; set; }

        public SelectList? Events { get; set; }

        public SelectList? Specialists { get; set; }

        //Conflict message found will be passed back if the slot is taken
        public string? ConflictMessage { get; set; }


    }

    public class BookingSearchViewModel
    {
        public List<Booking> Bookings { get; set; } = new();

        [Display(Name = "Search (customer / venue / event")]
        public string? SearchTerm { get; set; }


        [Display(Name ="Status")]
        public string? StatusFilter { get; set; }

        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name ="To Date")]
        [DataType(DataType.Date)]

        public DateTime? ToDate { get; set; }

        [Display(Name = "Venue")]
        public int? VenueFilter { get; set; }

        public SelectList? Venues { get; set; }
    }
     
    // Dashboard Summary Numbers
    public class DashboardViewModel { 
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }

        public int ConfirmedBooking { get; set; }

        public int TotalVenues { get; set; }

        public int TotalCustomers { get; set; }

        public int PendingEvents { get; set; } //Events with no Venue yet

        public decimal RevenueThisMonth { get; set; }

        public List<Booking> UpcomingBookings { get; set; } = new();

        public List<Booking> RecentBookings { get; set; } = new();
    }
}
