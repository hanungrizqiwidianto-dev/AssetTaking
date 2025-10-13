using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public DashboardController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpGet("GetTopAssetIn")]
        public IActionResult GetTopAssetIn()
        {
            try
            {
                var topAssetIn = _context.TblMAssetIns
                    .OrderByDescending(a => a.Qty)
                    .Take(3)
                    .Select(a => new
                    {
                        a.Id,
                        a.NamaBarang,
                        a.KodeBarang,
                        a.NomorAsset,
                        a.KategoriBarang,
                        a.Qty,
                        a.CreatedAt,
                        a.CreatedBy
                    })
                    .ToList();

                return Ok(topAssetIn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving top asset in", error = ex.Message });
            }
        }

        [HttpGet("GetTopAssetOut")]
        public IActionResult GetTopAssetOut()
        {
            try
            {
                var topAssetOut = _context.TblMAssetOuts
                    .OrderByDescending(a => a.Qty)
                    .Take(3)
                    .Select(a => new
                    {
                        a.Id,
                        a.NamaBarang,
                        a.KodeBarang,
                        a.NomorAsset,
                        a.KategoriBarang,
                        a.Qty,
                        a.CreatedAt,
                        a.CreatedBy
                    })
                    .ToList();

                return Ok(topAssetOut);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving top asset out", error = ex.Message });
            }
        }

        [HttpGet("GetOldestAssets")]
        public IActionResult GetOldestAssets()
        {
            try
            {
                var oldestAssets = _context.TblTAssets
                    .Where(a => a.Status == 1) // Asset In
                    .OrderBy(a => a.TanggalMasuk)
                    .Take(3)
                    .Select(a => new
                    {
                        a.Id,
                        a.NamaBarang,
                        a.KodeBarang,
                        a.NomorAsset,
                        a.KategoriBarang,
                        a.TanggalMasuk,
                        a.Qty,
                        DaysStored = a.TanggalMasuk.HasValue ? 
                            (DateTime.Now - a.TanggalMasuk.Value).Days : 0
                    })
                    .ToList();

                return Ok(oldestAssets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving oldest assets", error = ex.Message });
            }
        }

        [HttpGet("GetAssetsByCategory")]
        public IActionResult GetAssetsByCategory([FromQuery] int? status = null)
        {
            try
            {
                var query = _context.TblTAssets.AsQueryable();
                
                if (status.HasValue)
                {
                    query = query.Where(a => a.Status == status.Value);
                }

                var assetsByCategory = query
                    .Where(a => !string.IsNullOrEmpty(a.KategoriBarang))
                    .GroupBy(a => a.KategoriBarang)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        TotalQty = g.Sum(x => x.Qty ?? 0)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                return Ok(assetsByCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving assets by category", error = ex.Message });
            }
        }

        [HttpGet("GetDashboardStats")]
        public IActionResult GetDashboardStats()
        {
            try
            {
                var totalAssets = _context.TblTAssets.Count();
                var totalAssetIn = _context.TblTAssets.Count(a => a.Status == 1);
                var totalAssetOut = _context.TblTAssets.Count(a => a.Status == 2);
                var totalCategories = _context.TblTAssets
                    .Where(a => !string.IsNullOrEmpty(a.KategoriBarang))
                    .Select(a => a.KategoriBarang)
                    .Distinct()
                    .Count();

                var monthlyAssetIn = _context.TblTAssets
                    .Where(a => a.Status == 1 && a.CreatedAt.HasValue && 
                               a.CreatedAt.Value.Month == DateTime.Now.Month &&
                               a.CreatedAt.Value.Year == DateTime.Now.Year)
                    .Count();

                var monthlyAssetOut = _context.TblTAssets
                    .Where(a => a.Status == 2 && a.CreatedAt.HasValue && 
                               a.CreatedAt.Value.Month == DateTime.Now.Month &&
                               a.CreatedAt.Value.Year == DateTime.Now.Year)
                    .Count();

                return Ok(new
                {
                    TotalAssets = totalAssets,
                    TotalAssetIn = totalAssetIn,
                    TotalAssetOut = totalAssetOut,
                    TotalCategories = totalCategories,
                    MonthlyAssetIn = monthlyAssetIn,
                    MonthlyAssetOut = monthlyAssetOut
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard stats", error = ex.Message });
            }
        }

        [HttpGet("GetMonthlyTrend")]
        public IActionResult GetMonthlyTrend()
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var monthlyData = new List<object>();

                for (int month = 1; month <= 12; month++)
                {
                    var assetInCount = _context.TblTAssets
                        .Count(a => a.Status == 1 && a.CreatedAt.HasValue &&
                                   a.CreatedAt.Value.Month == month &&
                                   a.CreatedAt.Value.Year == currentYear);

                    var assetOutCount = _context.TblTAssets
                        .Count(a => a.Status == 2 && a.CreatedAt.HasValue &&
                                   a.CreatedAt.Value.Month == month &&
                                   a.CreatedAt.Value.Year == currentYear);

                    monthlyData.Add(new
                    {
                        Month = month,
                        MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
                        AssetIn = assetInCount,
                        AssetOut = assetOutCount
                    });
                }

                return Ok(monthlyData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving monthly trend", error = ex.Message });
            }
        }
    }
}