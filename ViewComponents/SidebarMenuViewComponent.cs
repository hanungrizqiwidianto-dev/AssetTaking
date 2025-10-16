using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;

namespace AssetTaking.ViewComponents
{
    public class SidebarMenuViewComponent(DbRndAssetTakingContext context, IHttpContextAccessor httpContextAccessor) : ViewComponent
    {
        private readonly DbRndAssetTakingContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public IViewComponentResult Invoke()
        {
            var nrp = _httpContextAccessor.HttpContext?.Session.GetString("Nrp");
            var roleId = _httpContextAccessor.HttpContext?.Session.GetInt32("ID_Role");

            if (string.IsNullOrEmpty(nrp) || roleId == null)
            {
                // kalau belum login ? return menu kosong
                return View("_Sidebar", new List<VwRMenu>());
            }

            // ambil menu sesuai role
            var menu = _context.VwRMenus
                               .Where(x => x.Id == roleId)
                               .OrderBy(x => x.Order)
                               .ToList();

            // ambil sub menu (contoh logic dari project lama)
            ViewBag.Sub = _context.TblRSubMenus
                                  .Where(x => x.Akses != null && x.Akses.Contains("ALL"))
                                  .ToList();

            return View("_Sidebar", menu);
        }
    }
}
