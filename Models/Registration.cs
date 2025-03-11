namespace carRentalProject.Models
{
    public class Registration
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
       
        public string PickupLocation { get; set; }
        public DateTime RentalTime { get; set; }
        public bool LookingForCar { get; set; }
    }
}