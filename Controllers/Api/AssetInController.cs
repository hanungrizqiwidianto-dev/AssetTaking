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

        [HttpGet("GetStates")]
        public async Task<IActionResult> GetStates()
        {
            try
            {
                var states = await _context.TblMStateCategories
                    .Where(x => !string.IsNullOrEmpty(x.State))
                    .OrderBy(x => x.State)
                    .Select(x => new
                    {
                        value = x.Id,
                        text = x.State
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = states });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetDistricts")]
        public async Task<IActionResult> GetDistricts()
        {
            try
            {
                var districts = await _context.TblMDstrctLocations
                    .Where(x => !string.IsNullOrEmpty(x.District))
                    .OrderBy(x => x.District)
                    .Select(x => new
                    {
                        value = x.District,
                        text = x.District
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = districts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("GenerateSerialNumbers")]
        public IActionResult GenerateSerialNumbers([FromBody] GenerateSerialNumbersRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.KategoriBarang) || request.Quantity <= 0)
                {
                    return BadRequest(new { success = false, message = "Kategori Barang dan Quantity harus diisi" });
                }

                var serialNumbers = GenerateSerialNumbers(request.KategoriBarang, request.Quantity);
                return Ok(new { 
                    success = true, 
                    serialNumbers = serialNumbers,
                    serialNumbersText = string.Join(", ", serialNumbers)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan saat generate serial numbers: " + ex.Message });
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
                        if (code != null)
                        {
                            var parts = code.Split('-');
                            if (parts.Length > 1 && int.TryParse(parts[1], out int num))
                                return num;
                        }
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
                    
                    // First create the AssetIn record
                    var assetIn = new TblTAssetIn
                    {
                        NamaBarang = request.NamaBarang,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        DstrctIn = request.DstrctIn,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };
                    _context.TblTAssetIns.Add(assetIn);
                    _context.SaveChanges(); // Save to get the AssetIn ID
                    
                    // Generate unique Id for primary key since it's not IDENTITY
                    var newId = GenerateNextAssetId();

                    var asset = new TblTAsset
                    {
                        Id = newId, // Set manual ID since it's not IDENTITY
                        // AssetId will be auto-generated by database (IDENTITY column)
                        AssetInId = assetIn.Id, // Set the AssetInId reference
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
                    
                    if (existingAssetIn != null && existingAssetIn.Id != assetIn.Id)
                    {
                        existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + request.Qty;
                        existingAssetIn.ModifiedAt = DateTime.Now;
                        existingAssetIn.ModifiedBy = "system";
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
                        DstrctIn = request.DstrctIn,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };

                    _context.TblTAssetIns.Add(assetIn);
                    _context.SaveChanges(); // Save to get the AssetIn ID

                    // Generate unique Id for primary key since it's not IDENTITY
                    var maxId = _context.TblTAssets.Max(a => (int?)a.Id) ?? 0;
                    var newId = maxId + 1;

                    var asset = new TblTAsset
                    {
                        Id = newId, // Set manual ID since it's not IDENTITY
                        // AssetId will be auto-generated by database (IDENTITY column)
                        AssetInId = assetIn.Id, // Set the AssetInId reference
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        Status = (int)StatusAsset.In,
                        PoNumber = request.PoNumber,
                        DstrctIn = request.DstrctIn,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };

                    _context.TblTAssets.Add(asset);
                }

                // Save changes to get the asset ID
                _context.SaveChanges();

                // Generate serial numbers for each quantity
                var savedAsset = await _context.TblTAssets
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                if (savedAsset != null && request.Qty > 0)
                {
                    List<string> serialNumbers;
                    
                    if (request.ManualSerial && !string.IsNullOrEmpty(request.SerialNumbers))
                    {
                        // Use manual serial numbers
                        serialNumbers = request.SerialNumbers
                            .Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                            
                        // Validate that we have enough serial numbers for the quantity
                        if (serialNumbers.Count != request.Qty)
                        {
                            return BadRequest(new { 
                                Remarks = false, 
                                Message = $"Jumlah serial number ({serialNumbers.Count}) harus sama dengan quantity ({request.Qty})" 
                            });
                        }
                        
                        // Check for duplicate serial numbers in input
                        var duplicateSerials = serialNumbers.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
                        if (duplicateSerials.Any())
                        {
                            return BadRequest(new { 
                                Remarks = false, 
                                Message = $"Serial number tidak boleh duplikat: {string.Join(", ", duplicateSerials)}" 
                            });
                        }
                        
                        // Check if serial numbers already exist in database
                        var existingSerials = await _context.TblRAssetSerials
                            .Where(s => serialNumbers.Contains(s.SerialNumber))
                            .Select(s => s.SerialNumber)
                            .ToListAsync();
                            
                        if (existingSerials.Any())
                        {
                            return BadRequest(new { 
                                Remarks = false, 
                                Message = $"Serial number sudah ada di database: {string.Join(", ", existingSerials)}" 
                            });
                        }
                    }
                    else
                    {
                        // Generate automatic serial numbers based on category
                        serialNumbers = GenerateSerialNumbers(request.KategoriBarang, request.Qty);
                    }
                    
                    // Create serial records
                    int firstSerialId = 0;
                    foreach (var serialNumber in serialNumbers)
                    {
                        var serialRecord = new TblRAssetSerial
                        {
                            AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                            SerialNumber = serialNumber,
                            StateId = request.StateId,
                            Status = 1, // Active
                            Notes = request.ManualSerial ? "Manual input" : $"Auto-generated for {request.KategoriBarang} category",
                            CreatedAt = DateTime.Now,
                            CreatedBy = "system"
                        };
                        _context.TblRAssetSerials.Add(serialRecord);
                        _context.SaveChanges(); // Save to get the SerialId
                        
                        // Store the first serial ID for reference
                        if (firstSerialId == 0)
                        {
                            firstSerialId = serialRecord.SerialId;
                        }
                    }
                    
                    // Create PO records if PoNumber and PoItem provided
                    if (!string.IsNullOrEmpty(request.PoNumber) && !string.IsNullOrEmpty(request.PoItem))
                    {
                        // Create PO records based on quantity and logic:
                        // If qty = 1, create one record with the PoNumber and PoItem
                        // If qty > 1, create qty records with same PoNumber but different PoItems
                        
                        if (request.Qty == 1)
                        {
                            // Single item - use the provided PoNumber and PoItem
                            var poRecord = new TblRAssetPo
                            {
                                AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                                PoNumber = request.PoNumber,
                                PoItem = request.PoItem,
                                CreatedAt = DateTime.Now,
                                CreatedBy = "system"
                            };
                            _context.TblRAssetPos.Add(poRecord);
                        }
                        else if (request.Qty > 1)
                        {
                            // Multiple items - same PoNumber but different PoItems
                            // Parse base PoItem to generate sequential items
                            string basePoItem = request.PoItem;
                            
                            for (int i = 0; i < request.Qty; i++)
                            {
                                string poItem;
                                
                                // Try to parse the PoItem and increment it
                                if (int.TryParse(basePoItem, out int poItemNumber))
                                {
                                    // If PoItem is numeric, increment it
                                    poItem = (poItemNumber + i).ToString();
                                }
                                else
                                {
                                    // If PoItem is not numeric, append sequence number
                                    poItem = $"{basePoItem}_{i + 1:D3}";
                                }
                                
                                var poRecord = new TblRAssetPo
                                {
                                    AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                                    PoNumber = request.PoNumber,
                                    PoItem = poItem,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = "system"
                                };
                                _context.TblRAssetPos.Add(poRecord);
                            }
                        }
                    }
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

                    // First create the AssetIn record
                    var assetIn = new TblTAssetIn
                    {
                        NamaBarang = request.NamaBarang,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        DstrctIn = request.DstrctIn,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };
                    _context.TblTAssetIns.Add(assetIn);
                    _context.SaveChanges(); // Save to get the AssetIn ID

                    // Generate unique Id for primary key since it's not IDENTITY
                    var maxId = _context.TblTAssets.Max(a => (int?)a.Id) ?? 0;
                    var newId = maxId + 1;

                    var asset = new TblTAsset
                    {
                        Id = newId, // Set manual ID since it's not IDENTITY
                        // AssetId will be auto-generated by database (IDENTITY column)
                        AssetInId = assetIn.Id, // Set the AssetInId reference
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        Status = (int)StatusAsset.In,
                        PoNumber = request.PoNumber,
                        DstrctIn = request.DstrctIn,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };

                    _context.TblTAssets.Add(asset);
                    
                    // Also update the asset in record if it exists
                    var existingAssetIn = _context.TblTAssetIns
                        .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);
                    
                    if (existingAssetIn != null && existingAssetIn.Id != assetIn.Id)
                    {
                        existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + request.Qty;
                        existingAssetIn.ModifiedAt = DateTime.Now;
                        existingAssetIn.ModifiedBy = "Scanner User";
                    }

                    _context.SaveChanges();
                    
                    // Get the saved asset for PO creation
                    var savedAsset = _context.TblTAssets
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);
                    
                    // Create PO records if PoNumber and PoItem provided
                    if (savedAsset != null && !string.IsNullOrEmpty(request.PoNumber) && !string.IsNullOrEmpty(request.PoItem))
                    {
                        // Create PO records based on quantity and logic:
                        // If qty = 1, create one record with the PoNumber and PoItem
                        // If qty > 1, create qty records with same PoNumber but different PoItems
                        
                        if (request.Qty == 1)
                        {
                            // Single item - use the provided PoNumber and PoItem
                            var poRecord = new TblRAssetPo
                            {
                                AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                                PoNumber = request.PoNumber,
                                PoItem = request.PoItem,
                                CreatedAt = DateTime.Now,
                                CreatedBy = "Scanner User"
                            };
                            _context.TblRAssetPos.Add(poRecord);
                        }
                        else if (request.Qty > 1)
                        {
                            // Multiple items - same PoNumber but different PoItems
                            // Parse base PoItem to generate sequential items
                            string basePoItem = request.PoItem;
                            
                            for (int i = 0; i < request.Qty; i++)
                            {
                                string poItem;
                                
                                // Try to parse the PoItem and increment it
                                if (int.TryParse(basePoItem, out int poItemNumber))
                                {
                                    // If PoItem is numeric, increment it
                                    poItem = (poItemNumber + i).ToString();
                                }
                                else
                                {
                                    // If PoItem is not numeric, append sequence number
                                    poItem = $"{basePoItem}_{i + 1:D3}";
                                }
                                
                                var poRecord = new TblRAssetPo
                                {
                                    AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                                    PoNumber = request.PoNumber,
                                    PoItem = poItem,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = "Scanner User"
                                };
                                _context.TblRAssetPos.Add(poRecord);
                            }
                        }
                        
                        _context.SaveChanges();
                    }
                    
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
                        DstrctIn = request.DstrctIn,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };

                    _context.TblTAssetIns.Add(assetIn);
                    _context.SaveChanges(); // Save to get the AssetIn ID

                    // Generate unique Id for primary key since it's not IDENTITY
                    var maxId = _context.TblTAssets.Max(a => (int?)a.Id) ?? 0;
                    var newId = maxId + 1;

                    var asset = new TblTAsset
                    {
                        Id = newId, // Set manual ID since it's not IDENTITY
                        // AssetId will be auto-generated by database (IDENTITY column)
                        AssetInId = assetIn.Id, // Set the AssetInId reference
                        NamaBarang = request.NamaBarang,
                        TanggalMasuk = DateTime.Now,
                        NomorAsset = request.NomorAsset,
                        KodeBarang = request.KodeBarang,
                        KategoriBarang = request.KategoriBarang,
                        Qty = request.Qty,
                        Foto = fotoPath,
                        Status = (int)StatusAsset.In,
                        PoNumber = request.PoNumber,
                        DstrctIn = request.DstrctIn,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Scanner User"
                    };

                    _context.TblTAssets.Add(asset);
                    
                    // Save changes to get the asset ID
                    _context.SaveChanges();

                    // Generate serial numbers for each quantity
                    var savedAsset = await _context.TblTAssets
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefaultAsync(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                    if (savedAsset != null && request.Qty > 0)
                    {
                        List<string> serialNumbers;
                        
                        if (request.ManualSerial && !string.IsNullOrEmpty(request.SerialNumbers))
                        {
                            // Use manual serial numbers
                            serialNumbers = request.SerialNumbers
                                .Split(',')
                                .Select(s => s.Trim())
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();
                                
                            // Validate that we have enough serial numbers for the quantity
                            if (serialNumbers.Count != request.Qty)
                            {
                                return Ok(new { 
                                    success = false, 
                                    message = $"Jumlah serial number ({serialNumbers.Count}) harus sama dengan quantity ({request.Qty})" 
                                });
                            }
                            
                            // Check for duplicate serial numbers in input
                            var duplicateSerials = serialNumbers.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
                            if (duplicateSerials.Any())
                            {
                                return Ok(new { 
                                    success = false, 
                                    message = $"Serial number tidak boleh duplikat: {string.Join(", ", duplicateSerials)}" 
                                });
                            }
                            
                            // Check if serial numbers already exist in database
                            var existingSerials = await _context.TblRAssetSerials
                                .Where(s => serialNumbers.Contains(s.SerialNumber))
                                .Select(s => s.SerialNumber)
                                .ToListAsync();
                                
                            if (existingSerials.Any())
                            {
                                return Ok(new { 
                                    success = false, 
                                    message = $"Serial number sudah ada di database: {string.Join(", ", existingSerials)}" 
                                });
                            }
                        }
                        else
                        {
                            // Generate automatic serial numbers based on category
                            serialNumbers = GenerateSerialNumbers(request.KategoriBarang, request.Qty);
                        }
                        
                        // Create serial records
                        int firstSerialId = 0;
                        foreach (var serialNumber in serialNumbers)
                        {
                            var serialRecord = new TblRAssetSerial
                            {
                                AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                                SerialNumber = serialNumber,
                                StateId = request.StateId,
                                Status = 1, // Active
                                Notes = $"Auto-generated for {request.KategoriBarang} category (from scan)",
                                CreatedAt = DateTime.Now,
                                CreatedBy = "Scanner User"
                            };
                            _context.TblRAssetSerials.Add(serialRecord);
                            _context.SaveChanges(); // Save to get the SerialId
                            
                            // Store the first serial ID for reference
                            if (firstSerialId == 0)
                            {
                                firstSerialId = serialRecord.SerialId;
                            }
                        }
                        
                        // Create PO records if PoNumber and PoItem provided
                        if (!string.IsNullOrEmpty(request.PoNumber) && !string.IsNullOrEmpty(request.PoItem))
                        {
                            // Create PO records based on quantity and logic:
                            // If qty = 1, create one record with the PoNumber and PoItem
                            // If qty > 1, create qty records with same PoNumber but different PoItems
                            
                            if (request.Qty == 1)
                            {
                                // Single item - use the provided PoNumber and PoItem
                                var poRecord = new TblRAssetPo
                                {
                                    AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                                    PoNumber = request.PoNumber,
                                    PoItem = request.PoItem,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = "Scanner User"
                                };
                                _context.TblRAssetPos.Add(poRecord);
                            }
                            else if (request.Qty > 1)
                            {
                                // Multiple items - same PoNumber but different PoItems
                                // Parse base PoItem to generate sequential items
                                string basePoItem = request.PoItem;
                                
                                for (int i = 0; i < request.Qty; i++)
                                {
                                    string poItem;
                                    
                                    // Try to parse the PoItem and increment it
                                    if (int.TryParse(basePoItem, out int poItemNumber))
                                    {
                                        // If PoItem is numeric, increment it
                                        poItem = (poItemNumber + i).ToString();
                                    }
                                    else
                                    {
                                        // If PoItem is not numeric, append sequence number
                                        poItem = $"{basePoItem}_{i + 1:D3}";
                                    }
                                    
                                    var poRecord = new TblRAssetPo
                                    {
                                        AssetId = savedAsset.AssetId, // Use AssetId instead of Id
                                        PoNumber = request.PoNumber,
                                        PoItem = poItem,
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = "Scanner User"
                                    };
                                    _context.TblRAssetPos.Add(poRecord);
                                }
                            }
                        }
                    }

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

        // Helper methods for serial number generation
        private List<string> GenerateSerialNumbers(string kategoriBarang, int quantity)
        {
            var serialNumbers = new List<string>();
            
            // Get category code
            string categoryCode = GetCategoryCode(kategoriBarang);
            
            // Get the last serial number for this category
            var lastSerial = _context.TblRAssetSerials
                .Where(s => s.SerialNumber.StartsWith(categoryCode))
                .OrderByDescending(s => s.SerialNumber)
                .FirstOrDefault();

            int nextNumber = 1;
            
            if (lastSerial != null)
            {
                // Extract the numeric part from the last serial number
                string numericPart = lastSerial.SerialNumber.Substring(categoryCode.Length);
                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }
            
            // Generate sequential serial numbers
            for (int i = 0; i < quantity; i++)
            {
                string serialNumber;
                bool isUnique;
                
                do
                {
                    // Format with 5 digits (padded with zeros)
                    serialNumber = $"{categoryCode}{nextNumber:D5}";
                    
                    // Check if this serial number already exists in database or in current list
                    isUnique = !_context.TblRAssetSerials.Any(s => s.SerialNumber == serialNumber) &&
                              !serialNumbers.Contains(serialNumber);
                    
                    if (!isUnique)
                    {
                        nextNumber++;
                    }
                } while (!isUnique);
                
                serialNumbers.Add(serialNumber);
                nextNumber++; // Increment for next serial number
            }
            
            return serialNumbers;
        }

        // Helper method to generate unique TblTAsset ID
        private int GenerateNextAssetId()
        {
            var maxId = _context.TblTAssets.Max(a => (int?)a.Id) ?? 0;
            return maxId + 1;
        }

        private string GetCategoryCode(string kategoriBarang)
        {
            // Use the same logic as GenerateItemCodeByCategory
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

            return prefix;
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
        public string? PoNumber { get; set; }
        public string? PoItem { get; set; }
        public int? StateId { get; set; }
        public string? DstrctIn { get; set; }
        public bool ManualSerial { get; set; } = false;
        public string? SerialNumbers { get; set; }
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

    public class GenerateSerialNumbersRequest
    {
        public string KategoriBarang { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
