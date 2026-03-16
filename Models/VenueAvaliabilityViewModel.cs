using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class VenueAvaliabilityViewModel
    {
        [Key]
        public int? ScheduleID { get; set; }

        [Required]
        [Display(Name = "Venue")]
        public int ? VenueID { get; set; }

        //Nullable - may be blocked for maintenance without a booking
        [Display(Name = "Booking (if applicable)")]
        public int? BookingID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name ="Date")]
        public DateTime? EventDate {  get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan? StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name ="End Time")]
        public TimeSpan? EndTime { get; set; }

        // Avaliable / Reserved / Maintenance
        [Required, StringLength(30)]
        public string? Status { get; set; } = "Available";

        [Display(Name = "Blocked (maintenance / hold")]
        public bool isBlocked { get; set; } = false;

        [Display(Name = "Block Reason")]
        public string? BlockReason { get; set; }

        //Referenced Table

        [ForeignKey("VenueID")]
        public VenueViewModel? Venue { get; set; }

        [ForeignKey("BookingID")]
        public Booking? Booking { get; set; }
    }
}
