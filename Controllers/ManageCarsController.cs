using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace carRentalProject.Controllers
{
    public class ManageCarsController : Controller
    {
        private readonly ILogger<ManageCarsController> _logger;
        private readonly MySqlConnection _connection;

        public ManageCarsController(ILogger<ManageCarsController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        // GET: CarBooking
        public IActionResult Index()
        {
          
            List<Cars> cars = new List<Cars>();

            try
            {
                _connection.Open();
             
                // Retrieve car types
                string carTypeQuery = "SELECT * FROM cars"; // Assuming the car types are in a table named "car_types"
                using (var cmd = new MySqlCommand(carTypeQuery, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new Cars
                        {
                            Id = reader.GetInt32("id"),
                            type = reader.GetString("type"),
                            model= reader.GetString("model"),
                            color = reader.GetString("color"),
                            location = reader.GetString("location"),
                            is_booked= reader.GetInt32("is_booked")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving cars: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to load car types.";
            }
            finally
            {
                _connection.Close();
            }

           
            ViewBag.CarDetails = cars; // Use ViewBag instead of ViewData
            return View();
        }


        // POST: UpdateReview
        [HttpPost]
        public IActionResult UpdateCars(int Id, string type, string model, string color, string location,int IsBooked)
        {
            try
            {
                // Open MySQL connection
                _connection.Open();

                // Update query
                string query = @"
                    UPDATE cars
                    SET type = @Type, model = @Model, color = @Color, location = @Location ,is_booked = @IsBookedCar
                    WHERE id = @Id";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@Model", model);
                    cmd.Parameters.AddWithValue("@Color", color);
                    cmd.Parameters.AddWithValue("@Location", location);
                    cmd.Parameters.AddWithValue("@IsBookedCar", IsBooked);

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Your Car has been updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating car: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update your car. Please try again.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index");
        }



        // GET: DeleteCars
        [HttpGet]
        public IActionResult DeleteCars(int id)
        {
            try
            {
                _connection.Open();

                string query = "DELETE FROM cars WHERE id = @Id";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Car deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting car: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to delete the car. Please try again.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index");
        }





    }
}
