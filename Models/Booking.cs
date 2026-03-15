using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string BookingID { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public string CustomerID { set; get; } //this is foregin Key for the Customer table

        [Required]
        [Display(Name = "Venue")]
        public string VenueID { set; get; } // this is a foregin Key for the Venue Table

        [Required]
        [Display(Name = "Event")]
        public string EventID { set; get; } // this is a foregin Key for the Event Table

        [Required]
        [Display(Name = "Captured By")]
        public string SpecialistID { set; get; } // this is a foregin Key for the BookingSpecialist Table 

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateOnly EventDate { set; get; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name ="Start Time")]
        public TimeOnly StartTime { set; get; }
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeOnly EndTime { set; get; }

        [Required, Range(1,10000)]
        [Display(Name = "Guest Count")]
        public int GuestCount { set; get; }

        [Display(Name = "Booking Date")]
        public DateTime BookingDate { set; get; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Price (R)")]
        public Double TotalPrice { set; get; }

        //Confirmed/ Pending / Cancelled
        [Required, StringLength(20)]
        public string Status { set; get; }

        [Display(Name ="Notes")]
        public string? Notes { get; set; } //this object will be if customer has requirements for the venue 

        //Foregin Table being referenced 
        [ForeignKey("CustomerID")]
        public CustomerViewModel? Customer { get; set; }

        [ForeignKey("VenueID")]
        public VenueViewModel? Venue { get; set; }

        [ForeignKey("EventID")]
        public EventViewModel? Event { get; set; }

        [ForeignKey("SpecialistID")]
        public BookingSpecialistViewModelcs? Specialist { get; set; }

        public VenueAvaliabilityViewModel? Schedule { get; set; }
    }
}
