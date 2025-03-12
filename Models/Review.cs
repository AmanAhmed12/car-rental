using System.ComponentModel.DataAnnotations;

namespace carRentalProject.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }  // Inquiry text

        [Required]
        public int MemberId { get; set; }  // Foreign Key (Assuming Member entity exists)

        [Required]
        public int CarTypeId { get; set; }  // Foreign Key (Referencing Car Type)
    }
}