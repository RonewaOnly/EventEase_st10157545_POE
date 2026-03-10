using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class Booking
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string BookingID { get; set; }

        public string CustomerID { set; get; } //this is foregin Key for the Customer table

        public string VenueID { set; get; } // this is a foregin Key for the Venue Table

        public string EventID { set; get; } // this is a foregin Key for the Event Table

        public string SpecialistID { set; get; } // this is a foregin Key for the BookingSpecialist Table 

        public DateOnly EventDate { set; get; }

        public TimeOnly StartTime { set; get; }

        public TimeOnly EndTime { set; get; }

        public int GuestCount { set; get; }

        public DateTime BookingDate { set; get; }

        public Double TotalPrice { set; get; }

        public string Status { set; get; }
    }
}
