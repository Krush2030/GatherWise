using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatherWise.Domain.Entities
{
    public class Slot
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Venue selection is required")]
        [ForeignKey("Venue")]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Slot date is required")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public bool IsBooked { get; set; } = false;

        // Navigation Property
        public Venue? Venue { get; set; }
    }
}