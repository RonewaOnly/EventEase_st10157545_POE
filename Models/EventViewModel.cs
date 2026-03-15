using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class EventViewModel
    {
        [Key]
        public int EventID { get; set; }

        [Required, StringLength(250)]
        [Display(Name = "Event Name")]
        public string? EventName { get; set; }


        //Allows an event to be captured before a venue is assigned
        [Display(Name = "Preferred Date")]
        [DataType(DataType.Date)]
        public DateTime? PreferredDate { get; set; }

        [Display(Name = "Description")]        
        public string? Description {  get; set; }

        [Display(Name = "Expected Guests")]
        [Range(1, 10000)]
        public int? ExpectedGuests { get; set;  }

        //Pending = no venue yet; Confirmed = venue booked; Cancelled
        [StringLength(25)]
        public string Status { get; set; } = "Pending";


        //Referenced Table
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
