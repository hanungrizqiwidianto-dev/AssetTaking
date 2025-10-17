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
                    data = serialNumbers,
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

        private async Task<string?> GetStateFromId(string? stateId)
        {
            if (string.IsNullOrEmpty(stateId) || !int.TryParse(stateId, out int id))
                return null;

            var state = await _context.TblMStateCategories
                .Where(x => x.Id == id)
                .Select(x => x.State)
                .FirstOrDefaultAsync();

            return state;
        }

        private async Task<string?> ResolveStateFromString(string? stateValue)
        {
            if (string.IsNullOrEmpty(stateValue))
                return null;

            // If State looks like a number (e.g., "1", "2"), treat it as StateId
            if (int.TryParse(stateValue, out int stateId))
            {
                return await GetStateFromId(stateValue);
            }
            // Otherwise, use it as the actual state name
            return stateValue;
        }

        private async Task<string?> ResolveState(AssetInRequest request)
        {
            // If State is provided directly, check if it's a number (might be StateId)
            if (!string.IsNullOrEmpty(request.State))
            {
                // If State looks like a number (e.g., "1", "2"), treat it as StateId
                if (int.TryParse(request.State, out int stateIdFromState))
                {
                    return await GetStateFromId(request.State);
                }
                // Otherwise, use it as the actual state name
                return request.State;
            }

            // If StateId is provided, resolve it to State name
            if (!string.IsNullOrEmpty(request.StateId))
                return await GetStateFromId(request.StateId);

            return null;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromForm] AssetInRequest request, IFormFile? fotoFile)
        {
            try
            {
                // Resolve state from StateId if needed
                var resolvedState = await ResolveState(request);
                
                // Validasi wajib isi State dan District In
                if (string.IsNullOrEmpty(resolvedState))
                {
                    return BadRequest(new { Remarks = false, Message = "State/Kondisi harus diisi" });
                }

                if (string.IsNullOrEmpty(request.DstrctIn))
                {
                    return BadRequest(new { Remarks = false, Message = "District In harus diisi" });
                }

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

                // Check if asset already exists in AssetIn table
                var existingAssetIn = _context.TblTAssetIns
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                TblTAssetIn assetIn;
                
                if (existingAssetIn != null)
                {
                    // Update existing AssetIn quantity instead of creating new record
                    existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + request.Qty;
                    existingAssetIn.ModifiedAt = DateTime.Now;
                    existingAssetIn.ModifiedBy = "system";
                    
                    // Update foto if new one is provided
                    if (!string.IsNullOrEmpty(fotoPath))
                    {
                        existingAssetIn.Foto = fotoPath;
                    }
                    
                    assetIn = existingAssetIn;
                }
                else
                {
                    // Create new AssetIn record
                    assetIn = new TblTAssetIn
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
                }
                
                _context.SaveChanges(); // Save to get/update the AssetIn ID

                // Always create new TblTAsset record for individual tracking
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
                    PoNumber = request.PoNumber,
                    DstrctIn = request.DstrctIn,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
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
                            State = !string.IsNullOrEmpty(resolvedState) ? resolvedState : "Good", // Default to "Good" if not provided
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
                                Status = 1, // Asset In status
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
                                    Status = 1, // Asset In status
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

                string message = existingAssetIn != null 
                    ? $"Asset berhasil diupdate. Qty ditambahkan ke asset yang sudah ada"
                    : "Asset In berhasil disimpan";

                return Ok(new { Remarks = true, Message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Remarks = false, Message = ex.Message });
            }
        }

        [HttpPost("CreateWithSteps")]
        public async Task<IActionResult> CreateWithSteps([FromForm] AssetInStepRequest request, IFormFile? fotoFile)
        {
            try
            {
                // Resolve the state to get proper state name
                var resolvedState = await ResolveStateFromString(request.State);
                
                // Validasi wajib isi State dan District In
                if (string.IsNullOrEmpty(request.State))
                {
                    return BadRequest(new { success = false, message = "State/Kondisi harus diisi" });
                }

                if (string.IsNullOrEmpty(request.DstrctIn))
                {
                    return BadRequest(new { success = false, message = "District In harus diisi" });
                }

                // Validasi serial numbers dan PO items
                if (request.SerialNumbers == null || request.SerialNumbers.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Serial numbers harus ada" });
                }

                if (request.SerialNumbers.Count != request.Qty)
                {
                    return BadRequest(new { success = false, message = "Jumlah serial numbers harus sama dengan quantity" });
                }

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
                        return BadRequest(new { success = false, message = "Format file tidak didukung. Gunakan JPG, JPEG, PNG, atau GIF." });
                    }

                    // Validate file size (5MB max)
                    if (fotoFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { success = false, message = "Ukuran file terlalu besar. Maksimal 5MB." });
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

                // Check if AssetIn already exists for the same code and increment quantity
                var existingAssetIn = _context.TblTAssetIns
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                TblTAssetIn assetIn;
                
                if (existingAssetIn != null)
                {
                    // Update existing AssetIn quantity instead of creating new record
                    existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + request.Qty;
                    existingAssetIn.ModifiedAt = DateTime.Now;
                    existingAssetIn.ModifiedBy = "system";
                    
                    // Update foto if new one is provided
                    if (!string.IsNullOrEmpty(fotoPath))
                    {
                        existingAssetIn.Foto = fotoPath;
                    }
                    
                    assetIn = existingAssetIn;
                }
                else
                {
                    // Create new AssetIn record
                    assetIn = new TblTAssetIn
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
                }
                
                _context.SaveChanges(); // Save to get/update the AssetIn ID

                // Create single asset record with full quantity (like the original logic)
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
                    Qty = request.Qty, // Full quantity in single record
                    Foto = fotoPath,
                    Status = (int)StatusAsset.In,
                    PoNumber = request.PoNumber,
                    DstrctIn = request.DstrctIn,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };
                _context.TblTAssets.Add(asset);
                _context.SaveChanges(); // Save to get the Asset ID

                // Create serial number records - one for each serial
                for (int i = 0; i < request.SerialNumbers.Count; i++)
                {
                    var serialRecord = new TblRAssetSerial
                    {
                        AssetId = asset.AssetId, // Use the auto-generated AssetId
                        SerialNumber = request.SerialNumbers[i],
                        State = !string.IsNullOrEmpty(resolvedState) ? resolvedState : "Good", // Use resolved state or default
                        Status = 1, // Asset In status
                        CreatedAt = DateTime.Now,
                        CreatedBy = "system"
                    };
                    _context.TblRAssetSerials.Add(serialRecord);
                }

                // Create PO records if PO Number provided - one record per serial number
                if (!string.IsNullOrEmpty(request.PoNumber))
                {
                    // Create PO record for each serial number (mapping 1:1 with serial numbers)
                    for (int i = 0; i < request.SerialNumbers.Count; i++)
                    {
                        // Get corresponding PO Item if exists, otherwise null
                        string? poItem = null;
                        if (request.PoItems != null && i < request.PoItems.Count)
                        {
                            // Treat empty strings as null for database consistency
                            poItem = string.IsNullOrWhiteSpace(request.PoItems[i]) ? null : request.PoItems[i].Trim();
                        }

                        var poRecord = new TblRAssetPo
                        {
                            AssetId = asset.AssetId, // Use the auto-generated AssetId
                            PoNumber = request.PoNumber,
                            PoItem = poItem, // Can be null if not provided
                            Status = 1, // Asset In status
                            CreatedAt = DateTime.Now,
                            CreatedBy = "system"
                        };
                        _context.TblRAssetPos.Add(poRecord);
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                return Ok(new { success = true, message = "Asset In berhasil disimpan dengan detail serial numbers dan PO items" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("CreateFromScan")]
        public async Task<IActionResult> CreateFromScan([FromForm] AssetInRequest request, IFormFile? fotoFile)
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

                // Resolve state from StateId if needed
                var resolvedState = await ResolveState(request);

                // Validasi wajib isi State dan District In untuk QR scan
                if (string.IsNullOrEmpty(resolvedState))
                {
                    return Ok(new { 
                        success = false, 
                        message = "State/Kondisi harus diisi" 
                    });
                }

                if (string.IsNullOrEmpty(request.DstrctIn))
                {
                    return Ok(new { 
                        success = false, 
                        message = "District In harus diisi" 
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
                
                try 
                {
                    // Check if asset already exists in AssetIn table
                    var existingAssetIn = _context.TblTAssetIns
                        .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                    TblTAssetIn assetIn;
                
                if (existingAssetIn != null)
                {
                    // Update existing AssetIn quantity instead of creating new record
                    existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + request.Qty;
                    existingAssetIn.ModifiedAt = DateTime.Now;
                    existingAssetIn.ModifiedBy = "Scanner User";
                    
                    // Update foto if new one is provided
                    if (!string.IsNullOrEmpty(fotoPath))
                    {
                        existingAssetIn.Foto = fotoPath;
                    }
                    
                    assetIn = existingAssetIn;
                }
                else
                {
                    // Create new AssetIn record
                    assetIn = new TblTAssetIn
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
                }
                
                _context.SaveChanges(); // Save to get/update the AssetIn ID

                // Always create new TblTAsset record for individual tracking
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

                _context.SaveChanges();
                
                // Get the saved asset for serial number and PO creation
                var savedAsset = _context.TblTAssets
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault(a => a.NomorAsset == request.NomorAsset && a.KodeBarang == request.KodeBarang);

                // Generate and save serial numbers for each quantity
                if (savedAsset != null && request.Qty > 0)
                {
                    List<string> serialNumbers;
                    
                    if (request.ManualSerial && !string.IsNullOrEmpty(request.SerialNumbers))
                    {
                        // Use manual serial numbers from QR scan
                        var inputSerials = request.SerialNumbers
                            .Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                            
                        // If qty is edited and we don't have enough serial numbers, auto-generate the rest
                        if (inputSerials.Count < request.Qty)
                        {
                            // Use existing serial numbers and generate additional ones
                            serialNumbers = new List<string>(inputSerials);
                            
                            // Generate additional serial numbers for the remaining quantity
                            int additionalCount = request.Qty - inputSerials.Count;
                            var additionalSerials = GenerateSerialNumbers(request.KategoriBarang, additionalCount);
                            serialNumbers.AddRange(additionalSerials);
                        }
                        else if (inputSerials.Count > request.Qty)
                        {
                            // Take only the first qty number of serials
                            serialNumbers = inputSerials.Take(request.Qty).ToList();
                        }
                        else
                        {
                            // Exact match
                            serialNumbers = inputSerials;
                        }
                        
                        // Check for duplicate serial numbers in final list
                        var duplicateSerials = serialNumbers.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
                        if (duplicateSerials.Any())
                        {
                            return Ok(new { 
                                success = false, 
                                message = $"QR Scan: Serial number tidak boleh duplikat: {string.Join(", ", duplicateSerials)}" 
                            });
                        }
                        
                        // Check if serial numbers already exist in database and make them unique
                        var existingSerials = await _context.TblRAssetSerials
                            .Where(s => serialNumbers.Contains(s.SerialNumber))
                            .Select(s => s.SerialNumber)
                            .ToListAsync();
                            
                        if (existingSerials.Any())
                        {
                            // Instead of failing, make serial numbers unique by appending suffix
                            for (int i = 0; i < serialNumbers.Count; i++)
                            {
                                if (existingSerials.Contains(serialNumbers[i]))
                                {
                                    // Find a unique suffix
                                    int suffix = 1;
                                    string baseSerial = serialNumbers[i];
                                    string newSerial;
                                    
                                    do
                                    {
                                        newSerial = $"{baseSerial}-{suffix:D2}";
                                        suffix++;
                                    }
                                    while (await _context.TblRAssetSerials.AnyAsync(s => s.SerialNumber == newSerial) || 
                                           serialNumbers.Contains(newSerial));
                                    
                                    serialNumbers[i] = newSerial;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Generate automatic serial numbers based on category and quantity
                        serialNumbers = GenerateSerialNumbers(request.KategoriBarang, request.Qty);
                    }
                    
                    // Create serial records for each quantity
                    foreach (var serialNumber in serialNumbers)
                    {
                        var serialRecord = new TblRAssetSerial
                        {
                            AssetId = savedAsset.AssetId, // Use AssetId (IDENTITY column)
                            SerialNumber = serialNumber,
                            State = !string.IsNullOrEmpty(resolvedState) ? resolvedState : "Good", // Use state from QR or default
                            Status = 1, // Active
                            Notes = request.ManualSerial ? "From QR scan (manual)" : $"From QR scan (auto-generated for {request.KategoriBarang})",
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Scanner User"
                        };
                        _context.TblRAssetSerials.Add(serialRecord);
                    }
                    
                    // Save serial records
                    _context.SaveChanges();
                }
                
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
                                Status = 1, // Asset In status
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
                                    Status = 1, // Asset In status
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
                        message = $"Asset scan berhasil diproses",
                        data = new {
                            id = savedAsset?.Id,
                            namaBarang = request.NamaBarang,
                            nomorAsset = request.NomorAsset,
                            qty = request.Qty
                        }
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Terjadi kesalahan saat menyimpan data: " + ex.Message 
                    });
                }
            }
            catch (Exception outerEx)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Terjadi kesalahan: " + outerEx.Message 
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
            if (string.IsNullOrWhiteSpace(kategoriBarang))
                return "GEN";

            // Remove any spaces and take first 3 characters, convert to uppercase
            string cleanCategory = kategoriBarang.Replace(" ", "").ToUpper();
            
            // Use first 3 characters as prefix, pad with 'X' if less than 3 characters
            string prefix = cleanCategory.Length >= 3 
                ? cleanCategory.Substring(0, 3) 
                : cleanCategory.PadRight(3, 'X');

            return prefix;
        }
    }  // End of AssetInController class

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
        public string? State { get; set; }
        public string? StateId { get; set; }
        public string? DstrctIn { get; set; }
        public bool ManualSerial { get; set; } = false;
        public string? SerialNumbers { get; set; }
    }

    public class AssetInStepRequest
    {
        public string NamaBarang { get; set; } = string.Empty;
        public string NomorAsset { get; set; } = string.Empty;
        public string KodeBarang { get; set; } = string.Empty;
        public string KategoriBarang { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string? PoNumber { get; set; }
        public string? State { get; set; }
        public string? DstrctIn { get; set; }
        public List<string> SerialNumbers { get; set; } = new List<string>();
        public List<string>? PoItems { get; set; } = new List<string>();
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
