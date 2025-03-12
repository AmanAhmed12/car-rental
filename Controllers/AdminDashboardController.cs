using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Diagnostics;

namespace carRentalProject.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ILogger<AdminDashboardController> _logger;
        private readonly MySqlConnection _connection;

        public AdminDashboardController(ILogger<AdminDashboardController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public IActionResult Index(string location, string carType)
        {
            List<Cars> cars = new List<Cars>();
            List<Cars> searchResult = new List<Cars>();
            try
            {
                _connection.Open();
                string query = "SELECT id, type,imageUrl,is_booked FROM cars "; // Retrieve all cars (no grouping)
                using (var cmd = new MySqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new Cars
                        {
                            Id = reader.GetInt32("id"),
                            type = reader.GetString("type"),
                            ImageUrl = reader.GetString("imageUrl"),
                            is_booked= reader.GetInt32(reader.GetOrdinal("is_booked"))
                        });
                    }
                }
                // If search parameters are provided, filter the cars
                if (!string.IsNullOrEmpty(location) || !string.IsNullOrEmpty(carType))
                {
                    string searchQuery = "SELECT id, type, model, color, imageUrl, location FROM cars WHERE 1=1";

                    if (!string.IsNullOrEmpty(location))
                    {
                        searchQuery += " AND location LIKE @location";
                    }
                    if (!string.IsNullOrEmpty(carType))
                    {
                        searchQuery += " AND type LIKE @carType";
                    }

                    using (var cmd = new MySqlCommand(searchQuery, _connection))
                    {
                        if (!string.IsNullOrEmpty(location))
                        {
                            cmd.Parameters.AddWithValue("@location", "%" + location + "%");
                        }
                        if (!string.IsNullOrEmpty(carType))
                        {
                            cmd.Parameters.AddWithValue("@carType", "%" + carType + "%");
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                searchResult.Add(new Cars
                                {
                                    Id = reader.GetInt32("id"),
                                    type = reader.GetString("type"),
                                    model = reader.GetString("model"),
                                    color = reader.GetString("color"),
                                    ImageUrl = reader.GetString("imageUrl"),
                                    location = reader.GetString("location")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving cars: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to load car details.";
            }
            finally
            {
                _connection.Close();
            }

            // Pass both available cars and search results to the view
         
            ViewData["SearchResult"] = searchResult;


            ViewBag.CarTypes = cars; // Use ViewBag instead of ViewData
            return View(cars);
        }


      



    }
}