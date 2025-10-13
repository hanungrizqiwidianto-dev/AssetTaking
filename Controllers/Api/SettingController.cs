using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using System.Text.Json;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public SettingController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpGet("Get_UserSetting")]
        public IActionResult Get_UserSetting()
        {
            try
            {
                var users = _context.VwUsers
                    .Select(u => new {
                        u.Username,
                        u.Name,
                        u.Email,
                        u.RoleName,
                        u.IdRole
                    })
                    .ToList();

                return Ok(new { Data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("Create_User")]
        public IActionResult Create_User([FromBody] CreateUserRequest request)
        {
            try
            {
                var newUser = new TblMUser
                {
                    Username = request.Username,
                    IdRole = request.IdRole
                };

                var user = _context.TblMUsers
                    .FirstOrDefault(u => u.IdRole == request.IdRole && u.Username == request.Username);

                if (user != null)
                {
                    return Ok(new { Remarks = false, Message = "User already exists" });
                }

                _context.TblMUsers.Add(newUser);
                _context.SaveChanges();

                return Ok(new { Remarks = true, Message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpDelete("Delete_User")]
        public IActionResult Delete_User(int role, string nrp)
        {
            try
            {
                var user = _context.TblMUsers
                    .FirstOrDefault(u => u.IdRole == role && u.Username == nrp);

                if (user != null)
                {
                    _context.TblMUsers.Remove(user);
                    _context.SaveChanges();
                    return Ok(new { Remarks = true, Message = "User deleted successfully" });
                }

                return Ok(new { Remarks = false, Message = "User not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpGet("Get_Menu/{roleId}")]
        public IActionResult Get_Menu(int roleId)
        {
            try
            {
                Console.WriteLine($"Get_Menu API called with roleId: {roleId}"); // Debug log

                // Get ALL menus from master table with their access status for specific role
                var menus = (from menu in _context.TblRMenus // Use master menu table
                           join akses in _context.TblMAkses on new { menu.IdMenu, IdRole = roleId } 
                               equals new { akses.IdMenu, akses.IdRole } into aksesGroup
                           from akses in aksesGroup.DefaultIfEmpty()
                           select new {
                               idMenu = menu.IdMenu,       // camelCase for JS
                               nameMenu = menu.NameMenu,   // camelCase for JS
                               linkMenu = menu.LinkMenu,   // camelCase for JS
                               iconMenu = menu.IconMenu,   // camelCase for JS
                               subMenu = menu.SubMenu,     // camelCase for JS
                               isAllow = akses != null ? akses.IsAllow : false // camelCase for JS
                           })
                           .OrderBy(m => m.idMenu) // Order by menu ID
                           .ToList();

                Console.WriteLine($"Found {menus.Count} menus for role {roleId}"); // Debug log
                
                // Log each menu for debugging
                foreach (var menu in menus)
                {
                    Console.WriteLine($"Menu: {menu.idMenu} - {menu.nameMenu} - IsAllow: {menu.isAllow}");
                }

                var response = new { Data = menus };
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get_Menu API Error: {ex.Message}"); // Debug log
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("Update_Menu")]
        public IActionResult Update_Menu([FromBody] UpdateMenuRequest request)
        {
            try
            {
                var akses = _context.TblMAkses
                    .FirstOrDefault(a => a.IdMenu == request.IdMenu && a.IdRole == request.IdRole);

                if (akses != null)
                {
                    akses.IsAllow = request.IsAllow;
                    _context.SaveChanges();
                    return Ok(new { Remarks = true, Message = "Menu access updated successfully" });
                }
                else
                {
                    // Create new access if not exists
                    var newAkses = new TblMAkse
                    {
                        IdMenu = request.IdMenu,
                        IdRole = request.IdRole,
                        IsAllow = request.IsAllow
                    };
                    _context.TblMAkses.Add(newAkses);
                    _context.SaveChanges();
                    return Ok(new { Remarks = true, Message = "Menu access created successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public int IdRole { get; set; }
    }

    public class UpdateMenuRequest
    {
        public int IdMenu { get; set; }
        public int IdRole { get; set; }
        public bool IsAllow { get; set; }
    }
}
