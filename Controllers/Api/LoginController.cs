using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using System.Text.Json;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public LoginController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpPost("Get_Login")]
        public IActionResult Get_Login([FromBody] LoginRequest request)
        {
            try
            {
                // Log the request
                Console.WriteLine($"Login attempt for username: {request?.Username}");
                
                if (request == null || string.IsNullOrEmpty(request.Username))
                {
                    return BadRequest(new { Remarks = false, Message = "Username is required" });
                }

                // Test database connection first
                var connectionTest = _context.Database.CanConnect();
                Console.WriteLine($"Database connection test: {connectionTest}");
                
                if (!connectionTest)
                {
                    return StatusCode(500, new { Remarks = false, Message = "Database connection failed" });
                }

                // Check total count in VwUsers
                var totalUsers = _context.VwUsers.Count();
                Console.WriteLine($"Total users in VwUsers: {totalUsers}");

                // Try to get all usernames for debugging (excluding nulls)
                var allUsernames = _context.VwUsers
                    .Where(u => u.Username != null)
                    .Select(u => u.Username).ToList();
                Console.WriteLine($"All usernames: {string.Join(", ", allUsernames)}");

                // Cek user di database (filter out null usernames first)
                var user = _context.VwUsers
                    .Where(u => u.Username != null && u.Username == request.Username)
                    .FirstOrDefault();

                Console.WriteLine($"User found: {user != null}");

                if (user != null)
                {
                    Console.WriteLine($"User details - ID: {user.IdRole}, Name: {user.Name}, Role: {user.RoleName}");
                    return Ok(new { Remarks = true, Message = "Login successful", User = new { user.Username, user.Name, user.RoleName } });
                }

                return Ok(new { Remarks = false, Message = "Username or Password incorrect" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Remarks = false, Message = ex.Message, Details = ex.StackTrace });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Jobsite { get; set; } = string.Empty;
    }
}
