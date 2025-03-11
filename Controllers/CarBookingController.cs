using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace carRentalProject.Controllers
{
    public class CarBookingController : Controller
    {
        private readonly ILogger<CarBookingController> _logger;
        private readonly MySqlConnection _connection;

        public CarBookingController(ILogger<CarBookingController> logger, MySqlConnection connection)
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
                string query = "SELECT id, type FROM cars WHERE is_booked = 0"; // Retrieve all cars (no grouping)
                using (var cmd = new MySqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new Cars
                        {
                            Id = reader.GetInt32("id"),
                            type = reader.GetString("type")
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
            return View();
        }

        [HttpPost]
        public IActionResult BookCar(int carTypeId, string name, string address, string telId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Retrieve the user ID from session
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to book a car.";
                    return RedirectToAction("Index");
                }

                _connection.Open();
                using (var transaction = _connection.BeginTransaction()) // Start a transaction
                {
                    try
                    {
                        // Insert booking details into booking_details table
                        string insertBookingQuery = "INSERT INTO booking_details (name, address, tel, dateFrom, dateTo, car_id, member_id) VALUES (@name, @address, @phone, @fromDate, @toDate, @carId, @memberId)";
                        using (var cmd = new MySqlCommand(insertBookingQuery, _connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.Parameters.AddWithValue("@address", address);
                            cmd.Parameters.AddWithValue("@phone", telId);
                            cmd.Parameters.AddWithValue("@fromDate", fromDate);
                            cmd.Parameters.AddWithValue("@toDate", toDate);
                            cmd.Parameters.AddWithValue("@carId", carTypeId);
                            cmd.Parameters.AddWithValue("@memberId", userId);

                            cmd.ExecuteNonQuery(); // Execute the insert query
                        }

                        // Update the 'is_booked' field of the car in the car table
                        string updateCarQuery = "UPDATE cars SET is_booked = 1 WHERE id = @carId";
                        using (var cmd = new MySqlCommand(updateCarQuery, _connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@carId", carTypeId);

                            cmd.ExecuteNonQuery(); // Execute the update query
                        }

                        // Commit the transaction if both operations succeed
                        transaction.Commit();

                        TempData["SuccessMessage"] = "Car booked successfully!";
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction if there is any error
                        transaction.Rollback();
                        _logger.LogError($"Error booking car: {ex.Message}");
                        TempData["ErrorMessage"] = "Failed to book the car.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error booking car: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to book the car.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

     

        public IActionResult Return(int carId)
        {
            try
            {
                _connection.Open();
                string updateQuery = "UPDATE cars SET is_booked = 0 WHERE id = @CarId";
                using (var cmd = new MySqlCommand(updateQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@CarId", carId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        TempData["SuccessMessage"] = "Car successfully returned!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to return the car.";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error returning car: {ex.Message}");
                TempData["ErrorMessage"] = "Error while returning the car.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index","AdminDashboard"); // Redirect back to the Index action to show updated car list
        }
    }
}
