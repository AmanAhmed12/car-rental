using MySqlConnector;
using Microsoft.AspNetCore.Mvc;

public class TestController : Controller
{
    private readonly MySqlConnection _mySqlConnection;

    public TestController(MySqlConnection mySqlConnection)
    {
        _mySqlConnection = mySqlConnection;
    }

    public IActionResult TestConnection()
    {
        try
        {
            _mySqlConnection.Open(); // Attempt to open the connection
            return Content("Database connection successful!");
        }
        catch (Exception ex)
        {
            return Content($"Database connection failed: {ex.Message}");
        }
        finally
        {
            _mySqlConnection.Close(); // Always close the connection
        }
    }
}
