using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using System.Text.Json;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MasterController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public MasterController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { Message = "API is working", Data = new[] { 
                new { Id = 1, RoleName = "Test Role 1" },
                new { Id = 2, RoleName = "Test Role 2" }
            }});
        }

        [HttpGet("GetRole")]
        public IActionResult GetRole()
        {
            try
            {
                Console.WriteLine("GetRole API called"); // Debug log
                
                var roles = _context.TblMRoles
                    .Select(r => new {
                        id = r.Id,
                        roleName = r.RoleName
                    })
                    .ToList();

                Console.WriteLine($"Found {roles.Count} roles"); // Debug log
                foreach (var role in roles)
                {
                    Console.WriteLine($"Role: {role.id} - {role.roleName}"); // Debug log
                }

                var response = new { data = roles };
                Console.WriteLine($"Returning response: {System.Text.Json.JsonSerializer.Serialize(response)}"); // Debug log
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRole API Error: {ex.Message}"); // Debug log
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("Get_Employee/{employeeId}")]
        public IActionResult Get_Employee(string employeeId)
        {
            try
            {
                Console.WriteLine($"Get_Employee API called with employeeId: {employeeId}"); // Debug log
                
                // Ambil data employee berdasarkan ID
                var employee = _context.TblRMasterKaryawanAlls
                    .Where(e => e.EmployeeId == employeeId)
                    .Select(e => new {
                        employeeId = e.EmployeeId,
                        name = e.Name,
                        deptCode = e.DeptCode,
                        email = e.Email,
                        posTitle = e.PosTitle
                    })
                    .FirstOrDefault();

                if (employee != null)
                {
                    Console.WriteLine($"Found employee: {employee.name}"); // Debug log
                    var response = new { data = employee };
                    return Ok(response);
                }
                else
                {
                    Console.WriteLine("Employee not found"); // Debug log
                    return NotFound(new { Message = "Employee not found" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get_Employee API Error: {ex.Message}"); // Debug log
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("Get_Employees/{roleName}")]
        public IActionResult Get_Employees(string roleName)
        {
            try
            {
                Console.WriteLine($"Get_Employees API called with roleName: {roleName}"); // Debug log
                
                // Ambil semua data employee untuk role tertentu
                var employees = _context.TblRMasterKaryawanAlls
                    .Select(e => new {
                        employeeId = e.EmployeeId,
                        name = e.Name,
                        deptCode = e.DeptCode,
                        email = e.Email,
                        posTitle = e.PosTitle
                    })
                    .ToList();

                Console.WriteLine($"Found {employees.Count} employees"); // Debug log

                var response = new { data = employees };
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get_Employees API Error: {ex.Message}"); // Debug log
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
