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
        public IActionResult Index(string location, string carType)
        {

            // Assuming the acs_type is stored in session
            var acsType = HttpContext.Session.GetString("AccountType");

            if (acsType == "admin")
            {
                return RedirectToAction("Index", "AdminDashboard");
            }
          
            List<Cars> cars = new List<Cars>();
            List<Cars> searchResult = new List<Cars>();
            List<Cars> carTypes = new List<Cars>();

            try
            {
                // Fetch all cars (Available Cars)
                _connection.Open();
                string query = "SELECT id, type, model, color, imageUrl, location FROM cars WHERE is_booked=0";
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


                // Fetch distinct car types
                string typeQuery = "SELECT * FROM cars";
                using (var cmd = new MySqlCommand(typeQuery, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        carTypes.Add(new Cars
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
            ViewBag.CarTypes = carTypes;
            ViewData["SearchResult"] = searchResult;
            return View(cars);
        }


        // POST: MemberDashboard/SubmitReview
        [HttpPost]
        public IActionResult SubmitReview(string Name, string Email, int carTypeId, string inquiryText)
        {
            try
            {
                // Retrieve member_id from session (Assuming it's stored in session)
                var memberId = HttpContext.Session.GetInt32("UserId");

                if (memberId == null)
                {
                    TempData["ErrorMessage"] = "You must be logged in to submit a review.";
                    return RedirectToAction("Index");
                }

                // Open MySQL connection
                _connection.Open();

                // Insert query
                string query = "INSERT INTO review (name, email, description, member_id, car_type_id) VALUES (@Name, @Email, @Description, @MemberId, @CarTypeId)";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Description", inquiryText);
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    cmd.Parameters.AddWithValue("@CarTypeId", carTypeId);

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Your review has been submitted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inserting review: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to submit your review. Please try again.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index","MemberDashboard");
        }
    }
}