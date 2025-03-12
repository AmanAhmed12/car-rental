using System.ComponentModel.DataAnnotations;

namespace carRentalProject.Models
{
    public class ReviewDetails
    {
       
        public int Id { get; set; }  // Primary Key

       
        public string Name { get; set; }

      
        public string Email { get; set; }

   
        public string Description { get; set; }  // Inquiry text

        public int MemberId { get; set; }  // Foreign Key (Assuming Member entity exists)

 
        public int CarTypeId { get; set; }  // Foreign Key (Referencing Car Type)

        public string CarType { get; set; }

        public string MemberName { get; set; }
    }
}