using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssetTaking.Models; // namespace hasil scaffolding DbContext/Models

namespace AssetTaking.Controllers
{
    public class SettingController(DbRndAssetTakingContext db) : Controller
    {
        private readonly DbRndAssetTakingContext _db = db;

        public IActionResult Users()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Emp = _db.TblRMasterKaryawanAlls.ToList();
            ViewBag.Group = _db.TblMRoles.ToList();

            return View();
        }

        public IActionResult Menu()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Group = _db.TblMRoles.ToList();
            return View();
        }
    }
}
