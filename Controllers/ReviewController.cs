using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;

namespace AssetTaking.Controllers
{
    public class ReviewController : Controller
    {
        private readonly DbRndAssetTakingContext _context;

        public ReviewController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
    }
}
