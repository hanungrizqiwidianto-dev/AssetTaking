using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using AssetTaking.Common;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetInController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public AssetInController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpPost("CheckDuplicate")]
        public IActionResult CheckDuplicate([FromBody] CheckDuplicateRequest request)
        {
            try
            {
                bool isDuplicate = false;
                string duplicateType = "";
                
                // Check nomor asset
                if (!string.IsNullOrEmpty(request.NomorAsset))
                {
                    var existingAssetByNumber = _context.TblTAssets
                        .FirstOrDefault(a => a.NomorAsset == request.NomorAsset);
                    if (existingAssetByNumber != null)
                    {
                        isDuplicate = true;
                        duplicateType = "Nomor Asset";
                    }
                }
                
                // Check kode barang
                if (!string.IsNullOrEmpty(request.KodeBarang) && !isDuplicate)
                {
                    var existingAssetByCode = _context.TblTAssets
                        .FirstOrDefault(a => a.KodeBarang == request.KodeBarang);
                    if (existingAssetByCode != null)
                    {
                        isDuplicate = true;
                        duplicateType = "Kode Barang";
                    }
                }

                return Ok(new { 
                    isDuplicate = isDuplicate,
                    duplicateType = duplicateType,
                    message = isDuplicate ? $"{duplicateType} sudah ada di database" : "Data valid"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    isDuplicate = false, 
                    message = "Terjadi kesalahan saat validasi: " + ex.Message 
                });
            }
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] AssetInRequest request)
        {
            try
            {
                using var transaction = _context.Database.BeginTransaction();

                var assetIn = new TblMAssetIn
                {
                    NamaBarang = request.NamaBarang,
                    NomorAsset = request.NomorAsset,
                    KodeBarang = request.KodeBarang,
                    KategoriBarang = request.KategoriBarang,
                    Qty = request.Qty,
                    Foto = request.Foto,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };

                _context.TblMAssetIns.Add(assetIn);
                _context.SaveChanges();

                var asset = new TblTAsset
                {
                    NamaBarang = request.NamaBarang,
                    TanggalMasuk = DateTime.Now,
                    NomorAsset = request.NomorAsset,
                    KodeBarang = request.KodeBarang,
                    KategoriBarang = request.KategoriBarang,
                    Qty = request.Qty,
                    Foto = request.Foto,
                    Status = (int)StatusAsset.In,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };

                _context.TblTAssets.Add(asset);
                _context.SaveChanges();

                transaction.Commit();

                return Ok(new { Remarks = true, Message = "Asset In berhasil disimpan" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpPost("CreateFromScan")]
        public IActionResult CreateFromScan([FromBody] AssetInRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(request.NamaBarang) || string.IsNullOrEmpty(request.NomorAsset))
                {
                    return Ok(new { 
                        success = false, 
                        message = "Nama Barang dan Nomor Asset harus diisi" 
                    });
                }

                // VALIDASI DUPLIKASI - BLOKIR JIKA ADA DUPLIKASI
                bool isDuplicate = false;
                string duplicateType = "";
                
                // Check nomor asset
                if (!string.IsNullOrEmpty(request.NomorAsset))
                {
                    var existingAssetByNumber = _context.TblTAssets
                        .FirstOrDefault(a => a.NomorAsset == request.NomorAsset);
                    if (existingAssetByNumber != null)
                    {
                        isDuplicate = true;
                        duplicateType = "Nomor Asset";
                    }
                }
                
                // Check kode barang
                if (!string.IsNullOrEmpty(request.KodeBarang) && !isDuplicate)
                {
                    var existingAssetByCode = _context.TblTAssets
                        .FirstOrDefault(a => a.KodeBarang == request.KodeBarang);
                    if (existingAssetByCode != null)
                    {
                        isDuplicate = true;
                        duplicateType = "Kode Barang";
                    }
                }

                // Jika ada duplikasi, tolak request
                if (isDuplicate)
                {
                    return Ok(new { 
                        success = false, 
                        message = $"{duplicateType} '{(duplicateType == "Nomor Asset" ? request.NomorAsset : request.KodeBarang)}' sudah terdaftar di database" 
                    });
                }

                using var transaction = _context.Database.BeginTransaction();

                var assetIn = new TblMAssetIn
                {
                    NamaBarang = request.NamaBarang,
                    NomorAsset = request.NomorAsset,
                    KodeBarang = request.KodeBarang,
                    KategoriBarang = request.KategoriBarang,
                    Qty = request.Qty,
                    Foto = request.Foto,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Scanner User" // Could be updated to use session user
                };

                _context.TblMAssetIns.Add(assetIn);
                _context.SaveChanges();

                var asset = new TblTAsset
                {
                    NamaBarang = request.NamaBarang,
                    TanggalMasuk = DateTime.Now,
                    NomorAsset = request.NomorAsset,
                    KodeBarang = request.KodeBarang,
                    KategoriBarang = request.KategoriBarang,
                    Qty = request.Qty,
                    Foto = request.Foto,
                    Status = (int)StatusAsset.In,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Scanner User"
                };

                _context.TblTAssets.Add(asset);
                _context.SaveChanges();

                transaction.Commit();

                return Ok(new { 
                    success = true, 
                    message = "Asset In berhasil disimpan melalui scan QR/Barcode",
                    data = new {
                        id = assetIn.Id,
                        namaBarang = assetIn.NamaBarang,
                        nomorAsset = assetIn.NomorAsset,
                        qty = assetIn.Qty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Terjadi kesalahan saat menyimpan data: " + ex.Message 
                });
            }
        }
    }

    public class AssetInRequest
    {
        public string NamaBarang { get; set; } = string.Empty;
        public string NomorAsset { get; set; } = string.Empty;
        public string KodeBarang { get; set; } = string.Empty;
        public string KategoriBarang { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string? Foto { get; set; }
    }

    public class CheckDuplicateRequest
    {
        public string? NomorAsset { get; set; }
        public string? KodeBarang { get; set; }
    }
}
