using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using AssetTaking.Common;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.TblMAssetCategories
                    .Where(x => !string.IsNullOrEmpty(x.KategoriBarang))
                    .OrderBy(x => x.KategoriBarang)
                    .Select(x => new
                    {
                        value = x.KategoriBarang,
                        text = x.KategoriBarang
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("GenerateItemCode")]
        public IActionResult GenerateItemCode([FromBody] GenerateItemCodeRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.KategoriBarang))
                {
                    return BadRequest(new { success = false, message = "Kategori Barang harus diisi" });
                }

                var itemCode = GenerateItemCodeByCategory(request.KategoriBarang);
                return Ok(new { success = true, itemCode = itemCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan saat generate kode barang: " + ex.Message });
            }
        }

        private string GenerateItemCodeByCategory(string kategoriBarang)
        {
            string prefix = "";
            
            // Generate prefix based on category
            switch (kategoriBarang.ToUpper())
            {
                case "RND":
                case "R&D":
                case "RESEARCH AND DEVELOPMENT":
                    prefix = "RND";
                    break;
                case "SPARE PART":
                case "SPAREPART":
                case "SP":
                    prefix = "SPA";
                    break;
                case "ELEKTRONIK":
                case "ELECTRONIC":
                case "ELK":
                    prefix = "ELK";
                    break;
                case "FURNITURE":
                case "FURNITUR":
                case "FRN":
                    prefix = "FRN";
                    break;
                case "KENDARAAN":
                case "VEHICLE":
                case "VHC":
                    prefix = "VHC";
                    break;
                case "PERALATAN":
                case "EQUIPMENT":
                case "EQP":
                    prefix = "EQP";
                    break;
                default:
                    // Use first 3 characters of category as prefix
                    prefix = kategoriBarang.Length >= 3 
                        ? kategoriBarang.Substring(0, 3).ToUpper() 
                        : kategoriBarang.ToUpper();
                    break;
            }

            // Get the next number for this category
            var existingCodes = _context.TblTAssets
                .Where(a => a.KodeBarang != null && a.KodeBarang.StartsWith(prefix + "-"))
                .Select(a => a.KodeBarang)
                .ToList();

            int nextNumber = 1;
            if (existingCodes.Any())
            {
                var numbers = existingCodes
                    .Select(code => 
                    {
                        var parts = code.Split('-');
                        if (parts.Length > 1 && int.TryParse(parts[1], out int num))
                            return num;
                        return 0;
                    })
                    .Where(num => num > 0);

                if (numbers.Any())
                {
                    nextNumber = numbers.Max() + 1;
                }
            }

            return $"{prefix}-{nextNumber}";
        }

        [HttpPost("CheckDuplicate")]
        public IActionResult CheckDuplicate([FromBody] CheckDuplicateRequest request)
        {
            try
            {
                // New logic: Check if asset exists and return information for quantity increment
                var existingAsset = _context.TblTAssets
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                if (existingAsset != null)
                {
                    return Ok(new { 
                        isDuplicate = true,
                        existingQty = existingAsset.Qty,
                        message = $"Asset dengan Nomor '{request.NomorAsset}' dan Kode '{request.KodeBarang}' sudah ada. Qty akan ditambahkan ke qty yang sudah ada."
                    });
                }

                return Ok(new { 
                    isDuplicate = false,
                    existingQty = 0,
                    message = "Data valid - akan dibuat sebagai asset baru"
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
        public async Task<IActionResult> Create([FromForm] AssetInRequest request, IFormFile? fotoFile)
        {
            try
            {
                using var transaction = _context.Database.BeginTransaction();

                string? fotoPath = null;

                // Handle file upload if provided
                if (fotoFile != null && fotoFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(fotoFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new { Remarks = false, Message = "Format file tidak didukung. Gunakan JPG, JPEG, PNG, atau GIF." });
                    }

                    // Validate file size (5MB max)
                    if (fotoFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { Remarks = false, Message = "Ukuran file terlalu besar. Maksimal 5MB." });
                    }

                    // Create unique filename
                    var fileName = $"asset_in_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    
                    // Ensure directory exists
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoFile.CopyToAsync(stream);
                    }

                    // Store relative path
                    fotoPath = $"/uploads/{fileName}";
                }

                // Check if asset already exists
                var existingAsset = _context.TblTAssets
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                if (existingAsset != null)
                {
                    // // Update existing asset quantity
                    // existingAsset.Qty = (existingAsset.Qty ?? 0) + request.Qty;
                    // existingAsset.ModifiedAt = DateTime.Now;
                    // existingAsset.ModifiedBy = "system";
                    
                    var asset = new TblTAsset
                    {
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        Status = (int)StatusAsset.In,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };

                    _context.TblTAssets.Add(asset);
                    
                    // Also update the asset in record if it exists
                    var existingAssetIn = _context.TblTAssetIns
                        .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);
                    
                    if (existingAssetIn != null)
                    {
                        existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + request.Qty;
                        existingAssetIn.ModifiedAt = DateTime.Now;
                        existingAssetIn.ModifiedBy = "system";
                    }
                    else
                    {
                        // Create new asset in record
                        var assetIn = new TblTAssetIn
                        {
                            NamaBarang = request.NamaBarang,
                            NomorAsset = request.NomorAsset,
                            KodeBarang = request.KodeBarang,
                            KategoriBarang = request.KategoriBarang,
                            Qty = request.Qty,
                            Foto = fotoPath,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "system"
                        };
                        _context.TblTAssetIns.Add(assetIn);
                    }
                }
                else
                {
                    // Create new asset and asset in records
                    var assetIn = new TblTAssetIn
                    {
                        NamaBarang = request.NamaBarang,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };

                    _context.TblTAssetIns.Add(assetIn);

                    var asset = new TblTAsset
                    {
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        Status = (int)StatusAsset.In,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };

                    _context.TblTAssets.Add(asset);
                }

                _context.SaveChanges();
                transaction.Commit();

                string message = existingAsset != null 
                    ? $"Asset berhasil diupdate. Qty ditambahkan ke asset yang sudah ada"
                    : "Asset In berhasil disimpan";

                return Ok(new { Remarks = true, Message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpPost("CreateFromScan")]
        public async Task<IActionResult> CreateFromScan([FromForm] AssetInRequest request, IFormFile? fotoFile)
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

                string? fotoPath = null;

                // Handle file upload if provided
                if (fotoFile != null && fotoFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(fotoFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Ok(new { success = false, message = "Format file tidak didukung. Gunakan JPG, JPEG, PNG, atau GIF." });
                    }

                    // Validate file size (5MB max)
                    if (fotoFile.Length > 5 * 1024 * 1024)
                    {
                        return Ok(new { success = false, message = "Ukuran file terlalu besar. Maksimal 5MB." });
                    }

                    // Create unique filename
                    var fileName = $"asset_scan_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    
                    // Ensure directory exists
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoFile.CopyToAsync(stream);
                    }

                    // Store relative path
                    fotoPath = $"/uploads/{fileName}";
                }

                using var transaction = _context.Database.BeginTransaction();

                // Check if asset already exists
                var existingAsset = _context.TblTAssets
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                if (existingAsset != null)
                {
                    // // Update existing asset quantity
                    // existingAsset.Qty = (existingAsset.Qty ?? 0) + request.Qty;
                    // existingAsset.ModifiedAt = DateTime.Now;
                    // existingAsset.ModifiedBy = "Scanner User";

                    var asset = new TblTAsset
                    {
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        Status = (int)StatusAsset.In,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };

                    _context.TblTAssets.Add(asset);
                    
                    // Also update the asset in record if it exists
                    var existingAssetIn = _context.TblTAssetIns
                        .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);
                    
                    if (existingAssetIn != null)
                    {
                        existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + request.Qty;
                        existingAssetIn.ModifiedAt = DateTime.Now;
                        existingAssetIn.ModifiedBy = "Scanner User";
                    }
                    else
                    {
                        // Create new asset in record
                        var assetIn = new TblTAssetIn
                        {
                            NamaBarang = request.NamaBarang,
                            NomorAsset = request.NomorAsset,
                            KodeBarang = request.KodeBarang,
                            KategoriBarang = request.KategoriBarang,
                            Qty = request.Qty,
                            Foto = fotoPath,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Scanner User"
                        };
                        _context.TblTAssetIns.Add(assetIn);
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    return Ok(new { 
                        success = true, 
                        message = $"Asset berhasil diupdate melalui scan. Qty ditambahkan ke asset yang sudah ada",
                        data = new {
                            id = existingAsset.Id,
                            namaBarang = existingAsset.NamaBarang,
                            nomorAsset = existingAsset.NomorAsset,
                            qty = existingAsset.Qty
                        }
                    });
                }
                else
                {
                    // Create new asset and asset in records
                    var assetIn = new TblTAssetIn
                    {
                        NamaBarang = request.NamaBarang,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };

                    _context.TblTAssetIns.Add(assetIn);

                    var asset = new TblTAsset
                    {
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
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

    public class GenerateItemCodeRequest
    {
        public string KategoriBarang { get; set; } = string.Empty;
    }
}
