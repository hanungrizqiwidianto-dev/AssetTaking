using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DbRndAssetTakingContext _context;

        public CategoryController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");
            return View();
        }
    }
}