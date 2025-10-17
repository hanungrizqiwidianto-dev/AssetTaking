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

        public IActionResult Detail(string kode, string nomor)
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrEmpty(kode) || string.IsNullOrEmpty(nomor))
            {
                TempData["ErrorMessage"] = "Invalid parameters. Asset code and number are required.";
                return RedirectToAction("Index");
            }

            ViewData["KodeBarang"] = kode;
            ViewData["NomorAsset"] = nomor;
            ViewData["Title"] = $"Asset Detail - {kode} / {nomor}";

            return View();
        }
    }
}
