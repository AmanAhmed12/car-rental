using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace carRentalProject.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly MySqlConnection _connection;

        public LoginController(ILogger<LoginController> logger, MySqlConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string Email, string Password)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                TempData["ErrorMessage"] = "Email and Password are required.";
                return RedirectToAction("Index");
            }

            try
            {
                _connection.Open();
                string query = "SELECT id,pwd, acc_type FROM registration WHERE email = @Email";

                using (var cmd = new MySqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {

                            string hashedPassword = reader["pwd"].ToString();
                            string accountType = reader["acc_type"].ToString().ToLower(); // Normalize case
                            int userId = Convert.ToInt32(reader["id"]); // Get the user ID

                            var passwordHasher = new PasswordHasher<string>();
                            var result = passwordHasher.VerifyHashedPassword(Email, hashedPassword, Password);

                            if (result == PasswordVerificationResult.Success)
                            {

                                HttpContext.Session.SetInt32("UserId", userId);
                                HttpContext.Session.SetString("AccountType", accountType);
                                // Redirect based on account type
                                if (accountType == "member")
                                {
                                    return RedirectToAction("Index", "MemberDashboard");
                                }
                                else if (accountType == "admin")
                                {
                                    return RedirectToAction("Index", "AdminDashboard");
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Invalid account type.";
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                    }
                }

                TempData["ErrorMessage"] = "Invalid email or password.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while logging in.";
                return RedirectToAction("Index");
            }
            finally
            {
                _connection.Close();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Logout successful!";
            return RedirectToAction("Index","Login");
        }
    }
}
