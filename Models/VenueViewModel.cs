using System.Buffers.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class VenueViewModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? VenueID { get; set; }

        public string? VenueName { get; set; }

        public string? Location { get; set; }

        public int Capacity { get; set; }

        public string? ImageUrl { get; set; }
    }
}
