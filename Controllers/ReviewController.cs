using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace carRentalProject.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ILogger<ReviewController> _logger;
        private readonly MySqlConnection _connection;

        public ReviewController(ILogger<ReviewController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        // GET: CarBooking
        public IActionResult Index()
        {
            List<ReviewDetails> reviews = new List<ReviewDetails>();
            List<Cars> cars = new List<Cars>();

            try
            {
                _connection.Open();
                string query = @"
            SELECT r.id, r.name, r.email, r.description, r.member_id, r.car_type_id, 
                   c.type AS car_type, m.first_name AS member_name 
            FROM review r
            LEFT JOIN cars c ON r.car_type_id = c.id
            LEFT JOIN registration m ON r.member_id = m.id";
                using (var cmd = new MySqlCommand(query, _connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reviews.Add(new ReviewDetails
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name"),  // Assuming the name field is present in your query
                            Email = reader.GetString("email"),  // Assuming the email field is present in your query
                            Description = reader.GetString("description"),  // Assuming the description field is present in your query
                            MemberId = reader.GetInt32("member_id"),  // Assuming the memberId field is present in your query
                            CarTypeId = reader.GetInt32("car_type_id"),  // Assuming the carTypeId field is present in your query
                             CarType = reader.IsDBNull(reader.GetOrdinal("car_type")) ? null : reader.GetString("car_type"),
                            MemberName = reader.IsDBNull(reader.GetOrdinal("member_name")) ? null : reader.GetString("member_name")
                        });
                    }
                }

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

            ViewBag.ReviewDetails = reviews; // Use ViewBag instead of ViewData
            ViewBag.CarTypes = cars; // Use ViewBag instead of ViewData
            return View();
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

            return RedirectToAction("Index");
        }


        // POST: UpdateReview
        [HttpPost]
        public IActionResult UpdateReview(int Id, string Name, string Email, int CarTypeId, string Description)
        {
            try
            {
                // Open MySQL connection
                _connection.Open();

                // Update query
                string query = @"
                    UPDATE review 
                    SET name = @Name, email = @Email, description = @Description, car_type_id = @CarTypeId 
                    WHERE id = @Id";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", Id);
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Description", Description);
                    cmd.Parameters.AddWithValue("@CarTypeId", CarTypeId);

                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Your review has been updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating review: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update your review. Please try again.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index");
        }

        // GET: DeleteReview
        [HttpGet]
        public IActionResult DeleteReview(int id)
        {
            try
            {
                _connection.Open();

                string query = "DELETE FROM review WHERE id = @Id";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Review deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting review: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to delete the review. Please try again.";
            }
            finally
            {
                _connection.Close();
            }

            return RedirectToAction("Index");
        }



    }
}
