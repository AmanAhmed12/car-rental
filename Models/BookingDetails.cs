using System.ComponentModel.DataAnnotations;

namespace carRentalProject.Models
{
    public class BookingDetails
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Tel { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        [Required]
        public int CarTypeId { get; set; }
    }
}