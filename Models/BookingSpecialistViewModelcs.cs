using System.ComponentModel.DataAnnotations;

namespace EventEase_st10157545_POE.Models
{
    public class BookingSpecialistViewModelcs
    {
        [Key]
        public int SpecialistID {get; set;}

        [Required,StringLength(150)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set;}

        [Required,StringLength(250)]
        [Display(Name ="Last Name")]
        public string? LastName { get; set;}

        [Required,StringLength(250)]
        [EmailAddress]
        [Display(Name ="Email Address")]

        public string? Email {  get; set;}

        [Required]
        [Display(Name ="Password")]
        public string? Password { get; set; }

        //Admin or Specialist
        [StringLength(25)]
        public string Role { get; set; } = "Specialist";

        //Computed Helper
        public string FullName => $"{FirstName} {LastName}";

        //Referenced Tables
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public ICollection<AuditLogViewModel> AuditLog { get; set; } = new List<AuditLogViewModel>();
    }
}
