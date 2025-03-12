using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Data;
using System.Diagnostics;



namespace carRentalProject.Controllers
{
    public class RentCarController : Controller
    {
        private readonly ILogger<RentCarController> _logger;
        private readonly MySqlConnection _connection;


        public RentCarController(ILogger<RentCarController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
           
        }

        // GET: Car/Index
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Car/AddCar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CarRent(Cars car, IFormFile imageUrl)
        {
            Console.WriteLine($"Type: {car.type}, Model: {car.model}, Color: {car.color}, Location: {car.location}, url: {imageUrl?.FileName} ");

            if (ModelState.IsValid)
            {
                try
                {
                    _connection.Open();

                    // Handle file upload
                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imageUrl.FileName);

                        // Save the uploaded image to the images folder
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            imageUrl.CopyTo(stream);
                        }

                        // Save the image URL (relative path)
                        car.ImageUrl = "/images/" + imageUrl.FileName;
                    }
                    else
                    {
                        // Default image if no file is uploaded
                        car.ImageUrl = "images/default.jpg";
                    }

                    // Insert car details into database
                    string query = "INSERT INTO cars (type, model, color, location, imageUrl, is_booked) VALUES (@Type, @Model, @Color, @Location, @ImageUrl, @IsBooked)";
                    using (var cmd = new MySqlCommand(query, _connection))
                    {
                        cmd.Parameters.AddWithValue("@Type", car.type);
                        cmd.Parameters.AddWithValue("@Model", car.model);
                        cmd.Parameters.AddWithValue("@Color", car.color);
                        cmd.Parameters.AddWithValue("@Location", car.location);
                        cmd.Parameters.AddWithValue("@ImageUrl", car.ImageUrl);
                        cmd.Parameters.AddWithValue("@IsBooked", 0);

                        cmd.ExecuteNonQuery();
                    }

                    _connection.Close();
                    TempData["SuccessMessage"] = "Car added successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while adding car: {ex.Message}");
                    TempData["ErrorMessage"] = "Car registration failed!";
                }
                finally
                {
                    if (_connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                }
            }

            return RedirectToAction("Index");
        }





    }
}
