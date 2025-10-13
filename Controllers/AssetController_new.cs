using Microsoft.AspNetCore.Mvc;

namespace AssetTaking.Controllers
{
    public class ApprovalController : Controller
    {
        public IActionResult AssetIn()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        public IActionResult AssetOut()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }
    }
}
