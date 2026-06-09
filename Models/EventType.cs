using System.ComponentModel.DataAnnotations;

namespace EventEase_st10157545_POE.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeID { get; set; }
        [Required, StringLength(100)]
        [Display(Name = "Event Type")]
        public string TypeName { get; set; } = string.Empty;
        [StringLength(300)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        // Navigation
        public ICollection<EventViewModel> Events { get; set; } = new List<EventViewModel>();
    }
}
