using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class AuditLogViewModel
    {
        [Key]
        public int LogID { get; set; }

        [Required] 
        public int SpecialistID { get; set; }

        [Required, StringLength(50)]
        public string? Action { get; set; } //Create / Update / Cancel

        [Required, StringLength(50)]
        public string? TablesAffected { get; set; } //Booking /Venue /etc.

        public int RecordID { get; set; }

        public DateTime Timestamp { get; set; }

        public string? Details { get; set; }

        [ForeignKey("SpecialistID")]
        public BookingSpecialistViewModelcs? specialist { get; set;  }
    }
}
