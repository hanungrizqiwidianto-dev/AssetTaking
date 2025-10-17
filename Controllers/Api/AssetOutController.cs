using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using AssetTaking.Common;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Create([FromForm] AssetOutRequest request, IFormFile? fotoFile)
        {
            try
            {
                // Validasi wajib isi State dan District Out
                if (string.IsNullOrEmpty(request.State))
                {
                    return BadRequest(new { Remarks = false, Message = "State/Kondisi harus diisi" });
                }

                if (string.IsNullOrEmpty(request.DstrctOut))
                {
                    return BadRequest(new { Remarks = false, Message = "District Out harus diisi" });
                }

                // Validasi jumlah serial numbers yang dipilih harus sama dengan qty
                if (request.SelectedSerials != null && request.SelectedSerials.Count != request.Qty)
                {
                    return BadRequest(new { 
                        Remarks = false, 
                        Message = $"Jumlah serial numbers yang dipilih ({request.SelectedSerials.Count}) harus sama dengan quantity ({request.Qty})" 
                    });
                }

                using var transaction = _context.Database.BeginTransaction();

                var sourceAsset = _context.TblTAssetIns
                    .FirstOrDefault(a => a.Id == request.AssetInId);

                if (sourceAsset == null)
                {
                    return BadRequest(new { Remarks = false, Message = "Asset tidak ditemukan" });
                }

                // Get PoNumber from the related TblTAsset record
                var sourceAssetRecord = _context.TblTAssets
                    .FirstOrDefault(a => a.AssetInId == request.AssetInId);
                string? poNumber = sourceAssetRecord?.PoNumber;

                if (sourceAsset.Qty < request.Qty)
                {
                    return BadRequest(new { Remarks = false, Message = "Qty tidak mencukupi. Stok tersedia: " + sourceAsset.Qty });
                }

                string? fotoPath = sourceAsset.Foto; // Default to source asset's photo

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
                    var fileName = $"asset_out_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";
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

                // Check if asset out already exists for the same code
                var existingAssetOut = _context.TblTAssetOuts
                    .FirstOrDefault(a => a.NomorAsset == sourceAsset.NomorAsset && a.KodeBarang == sourceAsset.KodeBarang);

                TblTAssetOut assetOut;
                
                if (existingAssetOut != null)
                {
                    // Update existing AssetOut quantity instead of creating new record
                    existingAssetOut.Qty = (existingAssetOut.Qty ?? 0) + request.Qty;
                    existingAssetOut.ModifiedAt = DateTime.Now;
                    existingAssetOut.ModifiedBy = "system";
                    
                    // Update foto if new one is provided
                    if (!string.IsNullOrEmpty(fotoPath) && fotoPath != sourceAsset.Foto)
                    {
                        existingAssetOut.Foto = fotoPath;
                    }
                    
                    assetOut = existingAssetOut;
                }
                else
                {
                    // Create new AssetOut record
                    assetOut = new TblTAssetOut
                    {
                        NamaBarang = sourceAsset.NamaBarang,
                        NomorAsset = sourceAsset.NomorAsset,
                        KodeBarang = sourceAsset.KodeBarang,
                        KategoriBarang = sourceAsset.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        DstrctOut = request.DstrctOut,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };
                    _context.TblTAssetOuts.Add(assetOut);
                }
                
                _context.SaveChanges(); // Save to get/update the AssetOut ID

                // Handle multiple assets for each quantity with selected serial numbers
                for (int i = 0; i < request.Qty; i++)
                {
                    // Generate unique Id for primary key since it's not IDENTITY
                    var newId = GenerateNextAssetId();

                    var asset = new TblTAsset
                    {
                        Id = newId, // Set manual ID since it's not IDENTITY
                        // AssetId will be auto-generated by database (IDENTITY column)
                        AssetOutId = assetOut.Id, // Set the AssetOutId reference
                        NamaBarang = sourceAsset.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = sourceAsset.NomorAsset,
                        KodeBarang = sourceAsset.KodeBarang,
                        KategoriBarang = sourceAsset.KategoriBarang,
                        Qty = 1, // Each asset is individual
                        Foto = fotoPath,
                        Status = (int)StatusAsset.Out,
                        PoNumber = poNumber, // Set the PoNumber from source asset
                        DstrctOut = request.DstrctOut,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };
                    _context.TblTAssets.Add(asset);
                    _context.SaveChanges(); // Save to get the Asset ID

                    // Transfer the selected serial number to this asset
                    if (request.SelectedSerials != null && i < request.SelectedSerials.Count)
                    {
                        var serialToTransfer = _context.TblRAssetSerials
                            .FirstOrDefault(s => s.SerialId == request.SelectedSerials[i]);
                        
                        if (serialToTransfer != null)
                        {
                            serialToTransfer.AssetId = asset.AssetId; // Use the auto-generated AssetId
                            serialToTransfer.State = !string.IsNullOrEmpty(request.State) ? request.State : "Out"; // Use selected state or default to "Out"
                            serialToTransfer.Status = 2; // Set status to 2 for Out
                            serialToTransfer.ModifiedAt = DateTime.Now;
                            serialToTransfer.ModifiedBy = "system";
                        }
                    }
                }

                // Update qty di TblTAssetIn
                sourceAsset.Qty -= request.Qty;
                sourceAsset.ModifiedAt = DateTime.Now;
                sourceAsset.ModifiedBy = "system";

                // Update status of original assets to Out  
                // Note: We'll track status at serial level instead of asset level for this implementation

                _context.SaveChanges();
                transaction.Commit();

                return Ok(new { Remarks = true, Message = "Asset Out berhasil disimpan" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpPost("CreateFromScan")]
        public async Task<IActionResult> CreateFromScan([FromForm] AssetOutFromScanRequest request, IFormFile? fotoFile)
        {
            try
            {
                // Validate required fields for QR scan
                if (string.IsNullOrEmpty(request.NamaBarang) || 
                    string.IsNullOrEmpty(request.NomorAsset) || 
                    string.IsNullOrEmpty(request.KodeBarang))
                {
                    return Ok(new { 
                        success = false, 
                        message = "QR Scan harus mengandung Nama Barang, Nomor Asset, dan Kode Barang" 
                    });
                }

                // Validasi wajib isi State dan District Out untuk QR scan
                if (string.IsNullOrEmpty(request.State))
                {
                    return Ok(new { 
                        success = false, 
                        message = "State/Kondisi harus diisi" 
                    });
                }

                if (string.IsNullOrEmpty(request.DstrctOut))
                {
                    return Ok(new { 
                        success = false, 
                        message = "District Out harus diisi" 
                    });
                }

                // Validate that serial numbers are provided from QR
                if (string.IsNullOrEmpty(request.SerialNumbers))
                {
                    return Ok(new { 
                        success = false, 
                        message = "QR Scan: Serial numbers harus ada dalam QR code untuk Asset Out" 
                    });
                }

                using var transaction = _context.Database.BeginTransaction();

                // Find the source asset in
                var sourceAsset = _context.TblTAssetIns
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                if (sourceAsset == null)
                {
                    return Ok(new { 
                        success = false, 
                        message = "Asset In tidak ditemukan untuk asset ini" 
                    });
                }

                // Get PoNumber from the related TblTAsset record
                var sourceAssetRecord = _context.TblTAssets
                    .FirstOrDefault(a => a.AssetInId == sourceAsset.Id);
                string? poNumber = sourceAssetRecord?.PoNumber;

                if (sourceAsset.Qty < request.Qty)
                {
                    return Ok(new { 
                        success = false, 
                        message = $"Qty tidak mencukupi. Stok tersedia: {sourceAsset.Qty}" 
                    });
                }

                // Parse serial numbers from QR
                var serialNumbers = request.SerialNumbers
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                if (serialNumbers.Count != request.Qty)
                {
                    return Ok(new { 
                        success = false, 
                        message = $"QR Scan: Jumlah serial number ({serialNumbers.Count}) harus sama dengan quantity ({request.Qty})" 
                    });
                }

                // Validate that all serial numbers exist and are available (status = 1)
                var availableSerials = await _context.TblRAssetSerials
                    .Where(s => serialNumbers.Contains(s.SerialNumber) && s.Status == 1)
                    .ToListAsync();

                if (availableSerials.Count != serialNumbers.Count)
                {
                    var unavailableSerials = serialNumbers.Except(availableSerials.Select(s => s.SerialNumber));
                    return Ok(new { 
                        success = false, 
                        message = $"Serial number tidak tersedia atau sudah di-out: {string.Join(", ", unavailableSerials)}" 
                    });
                }

                string? fotoPath = sourceAsset.Foto; // Default to source asset's photo

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
                    var fileName = $"asset_out_scan_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{fileExtension}";
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

                // Check if asset out already exists for the same code
                var existingAssetOut = _context.TblTAssetOuts
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                TblTAssetOut assetOut;
                
                if (existingAssetOut != null)
                {
                    // Update existing AssetOut quantity instead of creating new record
                    existingAssetOut.Qty = (existingAssetOut.Qty ?? 0) + request.Qty;
                    existingAssetOut.ModifiedAt = DateTime.Now;
                    existingAssetOut.ModifiedBy = "Scanner User";
                    
                    // Update foto if new one is provided
                    if (!string.IsNullOrEmpty(fotoPath))
                    {
                        existingAssetOut.Foto = fotoPath;
                    }
                    
                    assetOut = existingAssetOut;
                }
                else
                {
                    // Create new AssetOut record
                    assetOut = new TblTAssetOut
                    {
                        NamaBarang = request.NamaBarang,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        DstrctOut = request.DstrctOut,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };
                    _context.TblTAssetOuts.Add(assetOut);
                }
                
                _context.SaveChanges(); // Save to get/update the AssetOut ID

                // Handle multiple assets for each quantity with specific serial numbers from QR
                for (int i = 0; i < request.Qty; i++)
                {
                    // Generate unique Id for primary key since it's not IDENTITY
                    var newId = GenerateNextAssetId();

                    var asset = new TblTAsset
                    {
                        Id = newId, // Set manual ID since it's not IDENTITY
                        // AssetId will be auto-generated by database (IDENTITY column)
                        AssetOutId = assetOut.Id, // Set the AssetOutId reference
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = 1, // Each asset is individual
                        Foto = fotoPath,
                        Status = (int)StatusAsset.Out,
                        PoNumber = poNumber, // Set the PoNumber from source asset
                        DstrctOut = request.DstrctOut,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };
                    _context.TblTAssets.Add(asset);
                    _context.SaveChanges(); // Save to get the Asset ID

                    // Transfer the specific serial number from QR to this asset
                    if (i < serialNumbers.Count)
                    {
                        var serialToTransfer = availableSerials
                            .FirstOrDefault(s => s.SerialNumber == serialNumbers[i]);
                        
                        if (serialToTransfer != null)
                        {
                            serialToTransfer.AssetId = asset.AssetId; // Use the auto-generated AssetId
                            serialToTransfer.State = !string.IsNullOrEmpty(request.State) ? request.State : "Out"; // Use selected state or default to "Out"
                            serialToTransfer.Status = 2; // Set status to 2 for Out
                            serialToTransfer.ModifiedAt = DateTime.Now;
                            serialToTransfer.ModifiedBy = "Scanner User";
                        }
                    }
                }

                // Update qty di TblTAssetIn
                sourceAsset.Qty -= request.Qty;
                sourceAsset.ModifiedAt = DateTime.Now;
                sourceAsset.ModifiedBy = "Scanner User";

                _context.SaveChanges();
                transaction.Commit();

                return Ok(new { 
                    success = true, 
                    message = "Asset Out berhasil disimpan melalui scan QR/Barcode",
                    data = new {
                        id = assetOut.Id,
                        namaBarang = assetOut.NamaBarang,
                        nomorAsset = assetOut.NomorAsset,
                        qty = assetOut.Qty
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
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
                var assets = _context.TblTAssetIns
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
                var query = _context.TblTAssetIns
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
                var asset = _context.TblTAssetIns
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

        [HttpGet("GetSerialNumbers")]
        public IActionResult GetSerialNumbers(int assetId)
        {
            try
            {
                // Direct query: Find serial numbers for assets linked to this AssetIn
                var serialNumbers = (from serial in _context.TblRAssetSerials
                                   join asset in _context.TblTAssets on serial.AssetId equals asset.AssetId
                                   where asset.AssetInId == assetId && asset.Status == (int)StatusAsset.In
                                   select new {
                                       serialId = serial.SerialId,
                                       serialNumber = serial.SerialNumber,
                                       assetId = serial.AssetId
                                   })
                                   .OrderBy(s => s.serialNumber)
                                   .ToList();

                return Ok(new { success = true, data = serialNumbers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Helper method to generate unique TblTAsset ID
        private int GenerateNextAssetId()
        {
            var maxId = _context.TblTAssets.Max(a => (int?)a.Id) ?? 0;
            return maxId + 1;
        }
    }

    public class AssetOutRequest
    {
        public int AssetInId { get; set; }
        public int Qty { get; set; }
        public string? Foto { get; set; }
        public string? State { get; set; }
        public string? DstrctOut { get; set; }
        public List<int>? SelectedSerials { get; set; }
    }

    public class AssetOutFromScanRequest
    {
        public string NamaBarang { get; set; } = string.Empty;
        public string NomorAsset { get; set; } = string.Empty;
        public string KodeBarang { get; set; } = string.Empty;
        public string KategoriBarang { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string? Foto { get; set; }
        public string? State { get; set; }
        public string? DstrctOut { get; set; }
        public string SerialNumbers { get; set; } = string.Empty; // Comma-separated serial numbers from QR
    }
}
