using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using AssetTaking.Common;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetOutController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public AssetOutController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AssetOutRequest request)
        {
            try
            {
                using var transaction = _context.Database.BeginTransaction();

                var sourceAsset = _context.TblMAssetIns
                    .FirstOrDefault(a => a.Id == request.AssetInId);

                if (sourceAsset == null)
                {
                    return BadRequest(new { Remarks = false, Message = "Asset tidak ditemukan" });
                }

                if (sourceAsset.Qty < request.Qty)
                {
                    return BadRequest(new { Remarks = false, Message = "Qty tidak mencukupi. Stok tersedia: " + sourceAsset.Qty });
                }

                var assetOut = new TblMAssetOut
                {
                    NamaBarang = sourceAsset.NamaBarang,
                    NomorAsset = sourceAsset.NomorAsset,
                    KodeBarang = sourceAsset.KodeBarang,
                    KategoriBarang = sourceAsset.KategoriBarang,
                    Qty = request.Qty,
                    Foto = request.Foto, // Allow custom foto for asset out
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };
                _context.TblMAssetOuts.Add(assetOut);

                var asset = new TblTAsset
                {
                    NamaBarang = sourceAsset.NamaBarang,
                    TanggalMasuk = DateTime.Now,
                    NomorAsset = sourceAsset.NomorAsset,
                    KodeBarang = sourceAsset.KodeBarang,
                    KategoriBarang = sourceAsset.KategoriBarang,
                    Qty = request.Qty,
                    Foto = request.Foto,
                    Status = (int)StatusAsset.Out, // Status 2 for Asset Out
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };
                _context.TblTAssets.Add(asset);

                // Update qty di TblMAssetIn
                sourceAsset.Qty -= request.Qty;
                sourceAsset.ModifiedAt = DateTime.Now;
                sourceAsset.ModifiedBy = "system";

                _context.SaveChanges();
                transaction.Commit();

                return Ok(new { Remarks = true, Message = "Asset Out berhasil disimpan" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpGet("GetAvailableAssets")]
        public IActionResult GetAvailableAssets()
        {
            try
            {
                var assets = _context.TblTAssets
                    .Where(a => a.Qty > 0)
                    .Select(a => new {
                        a.Id,
                        a.NamaBarang,
                        a.NomorAsset,
                        a.KodeBarang,
                        a.KategoriBarang,
                        a.Qty
                    })
                    .ToList();

                return Ok(new { data = assets });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("GetAssetsFromAssetIn")]
        public IActionResult GetAssetsFromAssetIn()
        {
            try
            {
                var assets = _context.TblMAssetIns
                    .Where(a => !string.IsNullOrEmpty(a.NomorAsset) && !string.IsNullOrEmpty(a.NamaBarang) && a.Qty > 0)
                    .Select(a => new {
                        a.Id,
                        DisplayText = $"{a.NomorAsset} - {a.NamaBarang}",
                        a.NomorAsset,
                        a.NamaBarang,
                        a.KodeBarang,
                        a.KategoriBarang,
                        a.Qty,
                        a.Foto
                    })
                    .OrderBy(a => a.NomorAsset)
                    .ToList();

                return Ok(new { data = assets });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("SearchAssets")]
        public IActionResult SearchAssets([FromQuery] string term = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.TblMAssetIns
                    .Where(a => !string.IsNullOrEmpty(a.NomorAsset) && !string.IsNullOrEmpty(a.NamaBarang) && a.Qty > 0);

                if (!string.IsNullOrEmpty(term))
                {
                    term = term.ToLower();
                    query = query.Where(a => 
                        (a.NomorAsset != null && a.NomorAsset.ToLower().Contains(term)) || 
                        (a.NamaBarang != null && a.NamaBarang.ToLower().Contains(term)) ||
                        (a.KodeBarang != null && a.KodeBarang.ToLower().Contains(term)) ||
                        (a.KategoriBarang != null && a.KategoriBarang.ToLower().Contains(term))
                    );
                }

                var totalCount = query.Count();
                var assets = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new {
                        id = a.Id,
                        text = $"{a.NomorAsset} - {a.NamaBarang} (Qty: {a.Qty})",
                        nomorAsset = a.NomorAsset,
                        namaBarang = a.NamaBarang,
                        kodeBarang = a.KodeBarang,
                        kategoriBarang = a.KategoriBarang,
                        qty = a.Qty,
                        foto = a.Foto
                    })
                    .OrderBy(a => a.nomorAsset)
                    .ToList();

                return Ok(new { 
                    results = assets,
                    pagination = new {
                        more = (page * pageSize) < totalCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("GetAssetInDetail/{id}")]
        public IActionResult GetAssetInDetail(int id)
        {
            try
            {
                var asset = _context.TblMAssetIns
                    .Where(a => a.Id == id)
                    .Select(a => new {
                        a.Id,
                        a.NomorAsset,
                        a.NamaBarang,
                        a.KodeBarang,
                        a.KategoriBarang,
                        a.Qty,
                        a.Foto
                    })
                    .FirstOrDefault();

                if (asset == null)
                {
                    return NotFound(new { Remarks = false, Message = "Asset tidak ditemukan" });
                }

                return Ok(new { Remarks = true, data = asset });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }
    }

    public class AssetOutRequest
    {
        public int AssetInId { get; set; }
        public int Qty { get; set; }
        public string? Foto { get; set; }
    }
}
