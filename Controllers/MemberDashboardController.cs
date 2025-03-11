using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Diagnostics;

namespace carRentalProject.Controllers
{
    public class MemberDashboardController : Controller
    {
        private readonly ILogger<MemberDashboardController> _logger;
        private readonly MySqlConnection _connection;

        public MemberDashboardController(ILogger<MemberDashboardController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        // GET: MemberDashboard/Index
        public IActionResult Index()
        {
            List<Cars> cars = new List<Cars>();

            try
            {
                _connection.Open();
                string query = "SELECT id, type, model, color, imageUrl,location FROM cars";
                using (var cmd = new MySqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cars.Add(new Cars
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
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving cars: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to load car details.";
            }
            finally
            {
                _connection.Close();
            }

            return View(cars);
        }
    }
}