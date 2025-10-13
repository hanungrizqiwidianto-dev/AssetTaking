using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public NotificationController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetNotificationHistory()
        {
            try
            {
                var sevenDaysAgo = DateTime.Now.AddDays(-7);
                var now = DateTime.Now;

                // Get Asset In data from last 7 days
                var assetInData = await _context.TblTAssetIns
                    .Where(x => x.CreatedAt.HasValue && x.CreatedAt.Value >= sevenDaysAgo && x.CreatedAt.Value <= now)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        id = x.Id,
                        type = "Asset In",
                        title = x.NamaBarang ?? "Item tidak diketahui",
                        description = $"Asset masuk: {x.NamaBarang ?? "Item tidak diketahui"} (Qty: {x.Qty ?? 0})",
                        assetNumber = x.NomorAsset ?? "",
                        quantity = x.Qty ?? 0,
                        createdAt = x.CreatedAt,
                        createdBy = x.CreatedBy ?? "",
                        icon = "fa-arrow-down"
                    })
                    .ToListAsync();

                // Get Asset Out data from last 7 days
                var assetOutData = await _context.TblTAssetOuts
                    .Where(x => x.CreatedAt.HasValue && x.CreatedAt.Value >= sevenDaysAgo && x.CreatedAt.Value <= now)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        id = x.Id,
                        type = "Asset Out",
                        title = x.NamaBarang ?? "Item tidak diketahui",
                        description = $"Asset keluar: {x.NamaBarang ?? "Item tidak diketahui"} (Qty: {x.Qty ?? 0})",
                        assetNumber = x.NomorAsset ?? "",
                        quantity = x.Qty ?? 0,
                        createdAt = x.CreatedAt,
                        createdBy = x.CreatedBy ?? "",
                        icon = "fa-arrow-up"
                    })
                    .ToListAsync();

                // Combine and sort by date
                var notifications = assetInData.Concat(assetOutData)
                    .OrderByDescending(x => x.createdAt)
                    .Take(20) // Limit to latest 20 notifications
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = notifications,
                    count = notifications.Count,
                    message = "Data berhasil diambil"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Terjadi kesalahan saat mengambil data notifikasi",
                    error = ex.Message,
                    data = new List<object>(),
                    count = 0
                });
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetNotificationCount()
        {
            try
            {
                var sevenDaysAgo = DateTime.Now.AddDays(-7);
                var now = DateTime.Now;

                var assetInCount = await _context.TblTAssetIns
                    .CountAsync(x => x.CreatedAt.HasValue && x.CreatedAt.Value >= sevenDaysAgo && x.CreatedAt.Value <= now);

                var assetOutCount = await _context.TblTAssetOuts
                    .CountAsync(x => x.CreatedAt.HasValue && x.CreatedAt.Value >= sevenDaysAgo && x.CreatedAt.Value <= now);

                var totalCount = assetInCount + assetOutCount;

                return Ok(new
                {
                    success = true,
                    count = totalCount,
                    assetInCount = assetInCount,
                    assetOutCount = assetOutCount,
                    message = "Count berhasil diambil"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Terjadi kesalahan saat mengambil jumlah notifikasi",
                    error = ex.Message,
                    count = 0
                });
            }
        }
    }
}