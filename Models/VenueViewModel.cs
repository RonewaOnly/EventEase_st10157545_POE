using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class VenueViewModel
    {
        [Key]
        public int? VenueID { get; set; }

        [Required, StringLength(250)]
        [Display(Name = "Venue Name")]
        public string? VenueName { get; set; }

        [Required, StringLength(350)]
        [Display(Name ="Location / Address")]
        public string? Location { get; set; }

        [Required, Range(1, 10000)]
        [Display(Name = "Capacity (guests)")]
        public int Capacity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Price Per Day (R)")]
        public decimal? PricePerDay { get; set; }

        [StringLength(500)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        //Referenced Table
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        public ICollection<VenueAvaliabilityViewModel> Schedules { get; set; } = new List<VenueAvaliabilityViewModel>();

    }
}
