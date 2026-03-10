using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase_st10157545_POE.Models
{
    public class EventViewModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }

        [Required]
        public string? EventName { get; set; }


        [DataType(DataType.Date)]
        [Required]
        public DateOnly EventDate { get; set; }

        [Required]
        public string? Description {  get; set; }  

        public string? VenueId { get; set; }
    }
}
