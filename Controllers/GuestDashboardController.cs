using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Diagnostics;

namespace carRentalProject.Controllers
{
    public class GuestDashboardController : Controller
    {
        private readonly ILogger<GuestDashboardController> _logger;
        private readonly MySqlConnection _connection;

        public GuestDashboardController(ILogger<GuestDashboardController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public IActionResult Index()
        {
            List<Cars> cars = new List<Cars>();

            try
            {
                _connection.Open();
                string query = "SELECT id, type,imageUrl FROM cars WHERE is_booked = 0"; // Retrieve all cars (no grouping)
                using (var cmd = new MySqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new Cars
                        {
                            Id = reader.GetInt32("id"),
                            type = reader.GetString("type"),
                            ImageUrl = reader.GetString("imageUrl")
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

            ViewBag.CarTypes = cars; // Use ViewBag instead of ViewData
            return View(cars);
        }


        [HttpPost]
        public IActionResult SubmitInquiry(string FirstName, string LastName, string Email, string inquiryText)
        {
            try
            {
            

                _connection.Open();
              
                {
                    try
                    {
                        // Insert booking details into booking_details table
                        string insertBookingQuery = "INSERT INTO inquiry (first_name, last_name, email,inquiry_desc) VALUES (@firstName, @lastName, @email, @inquiry)";
                        using (var cmd = new MySqlCommand(insertBookingQuery, _connection))
                        {
                            cmd.Parameters.AddWithValue("@firstName", FirstName);
                            cmd.Parameters.AddWithValue("@lastName", LastName);
                            cmd.Parameters.AddWithValue("@email", Email);
                            cmd.Parameters.AddWithValue("@inquiry", inquiryText);
                           

                            cmd.ExecuteNonQuery(); // Execute the insert query
                        }

                     


                        TempData["SuccessMessage"] = "Submitted Inquiry successfully!";
                    }
                    catch (Exception ex)
                    {
                      
                        _logger.LogError($"Error submitting inquiry: {ex.Message}");
                        TempData["ErrorMessage"] = "Failed to submit inquiry.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking car: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to submit inquiry.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index");
        }
    }
}