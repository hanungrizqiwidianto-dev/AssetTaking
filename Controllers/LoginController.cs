using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using AssetTaking.Models;

namespace AssetTaking.Controllers
{
    public class LoginController(DbRndAssetTakingContext context, IConfiguration config) : Controller
    {
        private readonly DbRndAssetTakingContext _context = context;
        private readonly IConfiguration _config = config;

        // GET: Login
        public IActionResult Index()
        {
            // simpan WebApp_Link ke Session supaya bisa dipakai di semua view/js
            HttpContext.Session.SetString("Web_Link",
                _config.GetValue<string>("AppSettings:WebApp_Link") ?? string.Empty);
            return View();
        }

        [HttpPost]
        public IActionResult MakeSession([FromBody] LoginRequest req)
        {
            string? nrp = req.NRP;
            
            if (string.IsNullOrEmpty(nrp))
            {
                return Json(new { Remarks = false, Message = "NRP tidak boleh kosong" });
            }

            var dataUser = _context.TblRMasterKaryawanAlls
                                   .FirstOrDefault(a => a.EmployeeId == nrp);
            var dataRole = _context.TblMUsers
                                   .FirstOrDefault(a => a.Username == nrp);
            var dataJobsite = _context.VwUsers
                                   .FirstOrDefault(a => a.Username == nrp && a.RoleName == req.Jobsite);

            if (dataRole != null)
            {
                if (dataJobsite == null)
                {
                    return Json(new { Remarks = false, Message = "Role tidak sesuai" });
                }

                HttpContext.Session.SetString("Web_Link",
                    _config.GetValue<string>("AppSettings:WebApp_Link") ?? string.Empty);
                HttpContext.Session.SetString("Nrp", nrp);
                HttpContext.Session.SetInt32("ID_Role", dataRole.IdRole);
                HttpContext.Session.SetString("Name", dataUser?.Name ?? "");
                HttpContext.Session.SetString("Site", req.Jobsite ?? "");
                HttpContext.Session.SetString("PositionID", dataUser?.PositionId ?? "");

                return Json(new { Remarks = true });
            }
            else
            {
                return Json(new { Remarks = false, Message = "Maaf anda tidak memiliki akses" });
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }

    public class LoginRequest
    {
        public string? NRP { get; set; }
        public string? Jobsite { get; set; }
    }
}
