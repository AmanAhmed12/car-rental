using Microsoft.AspNetCore.Mvc;
using carRentalProject.Models;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace carRentalProject.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly MySqlConnection _connection;

        // Constructor that accepts both ILogger and MySqlConnection
        public RegisterController(ILogger<RegisterController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }


        // GET: Register/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: Register/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registration(Registration registration)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the email already exists in the database
                    string checkEmailQuery = "SELECT COUNT(*) FROM registration WHERE email = @Email";

                    _connection.Open();
                    using (var checkCmd = new MySqlCommand(checkEmailQuery, _connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", registration.Email);
                        int emailCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (emailCount > 0)
                        {
                            TempData["ErrorMessage"] = "Email is already registered!";
                            _connection.Close();
                            return RedirectToAction("Index", "Register");
                        }
                    }

                    // Hash the password
                    var passwordHasher = new PasswordHasher<Registration>();
                    var hashedPassword = passwordHasher.HashPassword(registration, registration.Password);

                    // SQL query to insert registration data
                    string query = "INSERT INTO registration (first_name, last_name, email, pwd, acc_type, pickup_Location, rental_time, is_renting_car) " +
                                   "VALUES (@FirstName, @LastName, @Email, @Password, @AccountType, @PickupLocation, @RentalTime, @LookingForCar)";

                    // Create a command with parameters to prevent SQL injection
                    using (var cmd = new MySqlCommand(query, _connection))
                    {
                        cmd.Parameters.AddWithValue("@FirstName", registration.FirstName);
                        cmd.Parameters.AddWithValue("@LastName", registration.LastName);
                        cmd.Parameters.AddWithValue("@Email", registration.Email);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@AccountType", registration.AccountType);
                        cmd.Parameters.AddWithValue("@PickupLocation", registration.PickupLocation);
                        cmd.Parameters.AddWithValue("@RentalTime", registration.RentalTime);
                        cmd.Parameters.AddWithValue("@LookingForCar", registration.LookingForCar);

                        cmd.ExecuteNonQuery();
                    }

                    _connection.Close();
                    TempData["SuccessMessage"] = "Registration successful! Please log in to continue.";

                    return RedirectToAction("Index", "Login");

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error while registering: {ex.Message}");
                    TempData["ErrorMessage"] = "Registration failed!";
                }
                finally
                {
                    if (_connection.State == ConnectionState.Open)
                    {
                        _connection.Close();
                    }
                }
            }

            return RedirectToAction("Index", "Login");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
