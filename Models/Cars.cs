namespace carRentalProject.Models
{
    public class Cars
    {
        public int Id { get; set; }
        public string type { get; set; }
        public string model { get; set; }
        public string color { get; set; }
        public string ImageUrl { get; set; }

        public string location { get; set; }

        public int is_booked { get; set; }
        // Constructor to set default values
        public Cars()
        {
            ImageUrl = "images/car.jpg"; // Setting default value for ImageUrl
        }
    }
}