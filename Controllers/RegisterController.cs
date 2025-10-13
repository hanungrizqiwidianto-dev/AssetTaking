using Microsoft.AspNetCore.Mvc;

namespace AssetTaking.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }
            ViewBag.AppId = id;
            return View();
        }

    }
}
