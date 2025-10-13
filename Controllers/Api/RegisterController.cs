using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using System.Text.Json;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public RegisterController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] CreateAssetRequest request)
        {
            try
            {
                var newAsset = new TblTAsset
                {
                    NamaBarang = request.NamaBarang,
                    TanggalMasuk = request.TanggalMasuk,
                    NomorAsset = request.NomorAsset,
                    KodeBarang = request.KodeBarang,
                    KategoriBarang = request.KategoriBarang,
                    Qty = request.Qty,
                    Foto = request.Foto,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.Now
                };

                _context.TblTAssets.Add(newAsset);
                _context.SaveChanges();

                return Ok(new { 
                    Remarks = true, 
                    Message = "Asset created successfully",
                    Data = new { Id = newAsset.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var assets = _context.TblTAssets
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => new {
                        a.Id,
                        a.NamaBarang,
                        a.TanggalMasuk,
                        a.NomorAsset,
                        a.KodeBarang,
                        a.KategoriBarang,
                        a.Qty,
                        a.Foto,
                        a.CreatedBy,
                        a.CreatedAt
                    })
                    .ToList();

                return Ok(new { Data = assets });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var asset = _context.TblTAssets
                    .Where(a => a.Id == id)
                    .Select(a => new {
                        a.Id,
                        a.NamaBarang,
                        a.TanggalMasuk,
                        a.NomorAsset,
                        a.KodeBarang,
                        a.KategoriBarang,
                        a.Qty,
                        a.Foto,
                        a.CreatedBy,
                        a.CreatedAt
                    })
                    .FirstOrDefault();

                if (asset != null)
                {
                    return Ok(new { Data = asset });
                }

                return NotFound(new { Message = "Asset not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("Update")]
        public IActionResult Update([FromBody] UpdateAssetRequest request)
        {
            try
            {
                var asset = _context.TblTAssets.FirstOrDefault(a => a.Id == request.Id);
                
                if (asset != null)
                {
                    asset.NamaBarang = request.NamaBarang;
                    asset.TanggalMasuk = request.TanggalMasuk;
                    asset.NomorAsset = request.NomorAsset;
                    asset.KodeBarang = request.KodeBarang;
                    asset.KategoriBarang = request.KategoriBarang;
                    asset.Qty = request.Qty;
                    asset.Foto = request.Foto;
                    asset.ModifiedBy = request.ModifiedBy;
                    asset.ModifiedAt = DateTime.Now;

                    _context.SaveChanges();
                    return Ok(new { Remarks = true, Message = "Asset updated successfully" });
                }

                return NotFound(new { Remarks = false, Message = "Asset not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpGet("GetChartData")]
        public IActionResult GetChartData()
        {
            try
            {
                // Generate chart data for asset statistics
                var categoryStats = _context.TblTAssets
                    .GroupBy(a => a.KategoriBarang)
                    .Select(g => new {
                        Category = g.Key,
                        Count = g.Count(),
                        TotalQty = g.Sum(x => x.Qty ?? 0)
                    })
                    .ToList();

                var monthlyStats = _context.TblTAssets
                    .Where(a => a.CreatedAt.HasValue)
                    .GroupBy(a => new { 
                        Year = a.CreatedAt!.Value.Year, 
                        Month = a.CreatedAt!.Value.Month 
                    })
                    .Select(g => new {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

                return Ok(new { 
                    CategoryStats = categoryStats, 
                    MonthlyStats = monthlyStats 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }

    public class CreateAssetRequest
    {
        public string NamaBarang { get; set; } = string.Empty;
        public DateTime? TanggalMasuk { get; set; }
        public string NomorAsset { get; set; } = string.Empty;
        public string KodeBarang { get; set; } = string.Empty;
        public string KategoriBarang { get; set; } = string.Empty;
        public int? Qty { get; set; }
        public string Foto { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class UpdateAssetRequest
    {
        public int Id { get; set; }
        public string NamaBarang { get; set; } = string.Empty;
        public DateTime? TanggalMasuk { get; set; }
        public string NomorAsset { get; set; } = string.Empty;
        public string KodeBarang { get; set; } = string.Empty;
        public string KategoriBarang { get; set; } = string.Empty;
        public int? Qty { get; set; }
        public string Foto { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
    }
}
