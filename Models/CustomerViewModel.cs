using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class CustomerViewModel
    {
        [Key]
        public int? CustomerID { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Required,StringLength(250)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string? Email {  get; set; }

        [Required, StringLength(13)]
        
        [DataType(DataType.PhoneNumber)]
        [Display(Name ="Phone Number")]
        public string? phoneNumber {  get; set; }

        [Display(Name ="Registered On")]
        public DateTime CreateAt { get; set;  } = DateTime.Now;


        //Computed Helper
        public string FullName => $"{FirstName} {LastName}";


        //Referenced Table
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
