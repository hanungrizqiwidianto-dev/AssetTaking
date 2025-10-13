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
        public IActionResult GetTopAssetIn([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.TblTAssetIns.AsQueryable();

                // Apply date filter
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt <= endDate.Value.AddDays(1).AddTicks(-1));
                }

                var topAssetIn = query
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
        public IActionResult GetTopAssetOut([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.TblTAssetOuts.AsQueryable();

                // Apply date filter
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt <= endDate.Value.AddDays(1).AddTicks(-1));
                }

                var topAssetOut = query
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
        public IActionResult GetOldestAssets([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.TblTAssets.Where(a => a.Status == 1); // Asset In

                // Apply date filter
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt <= endDate.Value.AddDays(1).AddTicks(-1));
                }

                var oldestAssets = query
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
        public IActionResult GetAssetsByCategory([FromQuery] int? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.TblTAssets.AsQueryable();
                
                if (status.HasValue)
                {
                    query = query.Where(a => a.Status == status.Value);
                }

                // Apply date filter
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt <= endDate.Value.AddDays(1).AddTicks(-1));
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
        public IActionResult GetDashboardStats([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.TblTAssets.AsQueryable();

                // Apply date filter
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.CreatedAt <= endDate.Value.AddDays(1).AddTicks(-1));
                }

                var totalAssets = query.Count();
                var totalAssetIn = query.Count(a => a.Status == 1);
                var totalAssetOut = query.Count(a => a.Status == 2);
                var totalCategories = query
                    .Where(a => !string.IsNullOrEmpty(a.KategoriBarang))
                    .Select(a => a.KategoriBarang)
                    .Distinct()
                    .Count();

                // Use filtered data instead of monthly filter
                var filteredAssetIn = query.Count(a => a.Status == 1);
                var filteredAssetOut = query.Count(a => a.Status == 2);

                return Ok(new
                {
                    TotalAssets = totalAssets,
                    TotalAssetIn = totalAssetIn,
                    TotalAssetOut = totalAssetOut,
                    TotalCategories = totalCategories,
                    MonthlyAssetIn = filteredAssetIn,
                    MonthlyAssetOut = filteredAssetOut
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving dashboard stats", error = ex.Message });
            }
        }

        [HttpGet("GetMonthlyTrend")]
        public IActionResult GetMonthlyTrend([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var monthlyData = new List<object>();

                // If date filter is applied, use those dates
                if (startDate.HasValue && endDate.HasValue)
                {
                    var currentDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
                    var endDateMonth = new DateTime(endDate.Value.Year, endDate.Value.Month, 1);

                    while (currentDate <= endDateMonth)
                    {
                        var monthStart = currentDate;
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                        var assetInCount = _context.TblTAssets
                            .Count(a => a.Status == 1 && a.CreatedAt.HasValue &&
                                       a.CreatedAt.Value >= monthStart &&
                                       a.CreatedAt.Value <= monthEnd.AddDays(1).AddTicks(-1));

                        var assetOutCount = _context.TblTAssets
                            .Count(a => a.Status == 2 && a.CreatedAt.HasValue &&
                                       a.CreatedAt.Value >= monthStart &&
                                       a.CreatedAt.Value <= monthEnd.AddDays(1).AddTicks(-1));

                        monthlyData.Add(new
                        {
                            Month = currentDate.Month,
                            Year = currentDate.Year,
                            MonthName = currentDate.ToString("MMM yyyy"),
                            AssetIn = assetInCount,
                            AssetOut = assetOutCount
                        });

                        currentDate = currentDate.AddMonths(1);
                    }
                }
                else
                {
                    // Default behavior - current year
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
                            Year = currentYear,
                            MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
                            AssetIn = assetInCount,
                            AssetOut = assetOutCount
                        });
                    }
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