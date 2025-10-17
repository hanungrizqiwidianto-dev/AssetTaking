using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using Microsoft.EntityFrameworkCore;
using AssetTaking.Common;

namespace AssetTaking.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public ReviewController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpGet("GetAssets")]
        public IActionResult GetAssets()
        {
            try
            {
                var assets = _context.TblTAssets
                    .Include(a => a.TblRAssetSerials)
                    .Include(a => a.TblRAssetPos)
                    .GroupBy(a => new { a.KodeBarang, a.NomorAsset })
                    .Select(g => new
                    {
                        Id = g.First().Id, // Add ID for reference
                        AssetId = g.First().AssetId, // Add AssetId for serial number lookup
                        KodeBarang = g.Key.KodeBarang,
                        NomorAsset = g.Key.NomorAsset,
                        NamaBarang = g.First().NamaBarang,
                        KategoriBarang = g.First().KategoriBarang,
                        Status = g.First().Status,
                        StatusText = g.First().Status == 1 ? "Asset In" : g.First().Status == 2 ? "Asset Out" : "Unknown",
                        // Calculate Qty: Asset In (status 1) - Asset Out (status 2), minimum 0
                        Qty = Math.Max(0, 
                            g.Where(x => x.Status == 1).Sum(x => x.Qty ?? 0) - 
                            g.Where(x => x.Status == 2).Sum(x => x.Qty ?? 0)
                        ),
                        //SerialId = g.First().SerialId,
                        PoNumber = g.SelectMany(x => x.TblRAssetPos).FirstOrDefault() != null ? 
                                   g.SelectMany(x => x.TblRAssetPos).First().PoNumber : null,
                        PoItem = g.SelectMany(x => x.TblRAssetPos).FirstOrDefault() != null ? 
                                 g.SelectMany(x => x.TblRAssetPos).First().PoItem : null,
                        DstrctIn = g.First().DstrctIn,
                        DstrctOut = g.First().DstrctOut,
                        // Add state and district from serial records
                        State = g.SelectMany(x => x.TblRAssetSerials)
                               .Where(s => s.Status == 1 && !string.IsNullOrEmpty(s.State))
                               .Select(s => s.State)
                               .FirstOrDefault(),
                        District = g.First().DstrctIn, // Use DstrctIn as district
                        SerialNumbers = g.SelectMany(x => x.TblRAssetSerials)
                                         .Where(s => s.Status == 1)
                                         .Select(s => s.SerialNumber)
                                         .ToList(),
                        SerialCount = g.SelectMany(x => x.TblRAssetSerials)
                                       .Count(s => s.Status == 1),
                        CreatedAt = g.First().CreatedAt,
                        CreatedBy = g.First().CreatedBy
                    })
                    .ToList();

                return Ok(assets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving assets", error = ex.Message });
            }
        }

        [HttpGet("GetAssetDetails")]
        public IActionResult GetAssetDetails([FromQuery] string kodeBarang, [FromQuery] string nomorAsset)
        {
            try
            {
                var assetDetails = _context.TblTAssets
                    .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                    .Select(a => new
                    {
                        a.Id,
                        a.NamaBarang,
                        a.TanggalMasuk,
                        a.NomorAsset,
                        a.KodeBarang,
                        a.KategoriBarang,
                        a.Qty,
                        a.Foto,
                        a.Status,
                        StatusText = a.Status == 1 ? "Asset In" : a.Status == 2 ? "Asset Out" : "Unknown",
                        a.DstrctIn,
                        a.DstrctOut,
                        a.PoNumber,
                        // Dynamic district based on status
                        District = a.Status == 1 ? a.DstrctIn : a.Status == 2 ? a.DstrctOut : null,
                        a.CreatedAt,
                        a.CreatedBy,
                        a.ModifiedAt,
                        a.ModifiedBy
                    })
                    .ToList();

                if (!assetDetails.Any())
                {
                    return NotFound(new { message = "Asset not found" });
                }

                return Ok(assetDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving asset details", error = ex.Message });
            }
        }

        [HttpGet("GetAssetOverview")]
        public IActionResult GetAssetOverview([FromQuery] string kodeBarang, [FromQuery] string nomorAsset)
        {
            try
            {
                // Get overview data
                var assetGroup = _context.TblTAssets
                    .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                    .GroupBy(a => new { a.KodeBarang, a.NomorAsset })
                    .Select(g => new
                    {
                        kodeBarang = g.Key.KodeBarang,
                        nomorAsset = g.Key.NomorAsset,
                        namaBarang = g.First().NamaBarang,
                        kategori = g.First().KategoriBarang,
                        // Calculate current quantity: Asset In - Asset Out
                        totalQuantity = Math.Max(0, 
                            g.Where(x => x.Status == 1).Sum(x => x.Qty ?? 0) - 
                            g.Where(x => x.Status == 2).Sum(x => x.Qty ?? 0)
                        ),
                        // Get PO Number from any record
                        poNumber = g.First().PoNumber,
                        // Calculate district based on latest status
                        district = g.OrderByDescending(x => x.CreatedAt).First().Status == 1 
                                   ? g.OrderByDescending(x => x.CreatedAt).First().DstrctIn 
                                   : g.OrderByDescending(x => x.CreatedAt).First().DstrctOut,
                        createdAt = g.Min(x => x.CreatedAt),
                        updatedAt = g.Max(x => x.ModifiedAt ?? x.CreatedAt)
                    })
                    .FirstOrDefault();

                if (assetGroup == null)
                {
                    return NotFound(new { success = false, message = "Asset not found" });
                }

                // Get detailed assets list
                var assetDetails = _context.TblTAssets
                    .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                    .Select(a => new
                    {
                        id = a.Id,
                        namaBarang = a.NamaBarang,
                        tanggalMasuk = a.TanggalMasuk,
                        nomorAsset = a.NomorAsset,
                        kodeBarang = a.KodeBarang,
                        kategori = a.KategoriBarang,
                        qty = a.Qty,
                        foto = a.Foto,
                        status = a.Status,
                        statusAsset = a.Status == 1 ? "Asset In" : a.Status == 2 ? "Asset Out" : "Unknown",
                        dstrctIn = a.DstrctIn,
                        dstrctOut = a.DstrctOut,
                        poNumber = a.PoNumber,
                        // Dynamic district based on status
                        district = a.Status == 1 ? a.DstrctIn : a.Status == 2 ? a.DstrctOut : null,
                        createdAt = a.CreatedAt,
                        createdBy = a.CreatedBy,
                        modifiedAt = a.ModifiedAt,
                        modifiedBy = a.ModifiedBy
                    })
                    .OrderBy(a => a.id)
                    .ToList();

                var result = new { 
                    success = true, 
                    data = new {
                        overview = assetGroup,
                        assets = assetDetails
                    }
                };

                Console.WriteLine($"Returning {assetDetails.Count} assets for {kodeBarang}/{nomorAsset}");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving asset overview", error = ex.Message });
            }
        }

        [HttpGet("GetAssetDetailsPaginated")]
        public IActionResult GetAssetDetailsPaginated([FromQuery] string kodeBarang, [FromQuery] string nomorAsset, [FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            try
            {
                var query = _context.TblTAssets
                    .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                    .Select(a => new
                    {
                        a.Id,
                        a.NamaBarang,
                        a.TanggalMasuk,
                        a.NomorAsset,
                        a.KodeBarang,
                        a.KategoriBarang,
                        a.Qty,
                        a.Foto,
                        a.Status,
                        StatusText = a.Status == 1 ? "Asset In" : a.Status == 2 ? "Asset Out" : "Unknown",
                        a.DstrctIn,
                        a.DstrctOut,
                        a.PoNumber,
                        // Dynamic district based on status
                        District = a.Status == 1 ? a.DstrctIn : a.Status == 2 ? a.DstrctOut : null,
                        a.CreatedAt,
                        a.CreatedBy,
                        a.ModifiedAt,
                        a.ModifiedBy
                    });

                var totalRecords = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var assetDetails = query
                    .OrderBy(a => a.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (!assetDetails.Any() && totalRecords == 0)
                {
                    return NotFound(new { message = "Asset not found" });
                }

                return Ok(new
                {
                    data = assetDetails,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalRecords = totalRecords,
                        totalPages = totalPages,
                        hasNextPage = page < totalPages,
                        hasPreviousPage = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving asset details", error = ex.Message });
            }
        }

        [HttpPut("UpdateAsset/{id}")]
        public IActionResult UpdateAsset(int id, [FromBody] UpdateAssetFromReviewRequest request)
        {
            try
            {
                var asset = _context.TblTAssets.FirstOrDefault(a => a.Id == id);
                if (asset == null)
                {
                    return NotFound(new { success = false, message = "Asset tidak ditemukan" });
                }

                using var transaction = _context.Database.BeginTransaction();

                var oldQty = asset.Qty ?? 0;
                var newQty = request.Qty;
                var qtyDifference = newQty - oldQty;
                if (oldQty > newQty)
                {
                    qtyDifference = oldQty - newQty;
                }
                if (newQty > oldQty)
                {
                    qtyDifference = newQty - oldQty;
                }
                
                var isAssetIn = asset.Status == (int)StatusAsset.In;
                var isAssetOut = asset.Status == (int)StatusAsset.Out;

                if (isAssetIn)
                {
                    // Edit Asset In: Validasi tidak boleh kurang dari total Asset Out
                    var totalAssetOut = _context.TblTAssets
                        .Where(a => a.NomorAsset == asset.NomorAsset && 
                                   a.KodeBarang == asset.KodeBarang && 
                                   a.Status == (int)StatusAsset.Out)
                        .Sum(a => a.Qty ?? 0);
                    
                    if (newQty < totalAssetOut)
                    {
                        return BadRequest(new { 
                            success = false, 
                            message = $"Quantity Asset In ({newQty}) tidak boleh kurang dari total Asset Out yang sudah ada ({totalAssetOut})" 
                        });
                    }
                    
                    // Update qty di TblTAssetIn
                    var assetIn = _context.TblTAssetIns
                        .FirstOrDefault(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);
                    
                    if (assetIn != null)
                    {
                        if (newQty > oldQty)
                            assetIn.Qty += qtyDifference; // Set langsung ke quantity baru
                        else if (newQty < oldQty)
                            assetIn.Qty -= qtyDifference; // Set langsung ke quantity baru
                        assetIn.ModifiedAt = DateTime.Now;
                        assetIn.ModifiedBy = "system";
                    }
                }
                else if (isAssetOut)
                {
                    // Edit Asset Out: Validasi tidak boleh melebihi Asset In
                    var assetIn = _context.TblTAssetIns
                        .FirstOrDefault(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);
                    
                    if (assetIn != null)
                    {
                        var availableStock = assetIn.Qty ?? 0;
                        
                        // Hitung total Asset Out yang sudah ada (kecuali yang sedang diedit)
                        var otherAssetOutTotal = _context.TblTAssets
                            .Where(a => a.NomorAsset == asset.NomorAsset && 
                                       a.KodeBarang == asset.KodeBarang && 
                                       a.Status == (int)StatusAsset.Out &&
                                       a.Id != id)
                            .Sum(a => a.Qty ?? 0);
                        
                        // Total Asset Out setelah edit = Asset Out lain + Quantity baru
                        var newTotalOutAfterEdit = otherAssetOutTotal + newQty;
                        
                        // Validasi: Total Asset Out tidak boleh melebihi Asset In
                        if (newTotalOutAfterEdit > availableStock)
                        {
                            return BadRequest(new { 
                                success = false, 
                                message = $"Quantity tidak valid! Total Asset Out ({newTotalOutAfterEdit}) akan melebihi Asset In yang tersedia ({availableStock}). Quantity maksimal yang bisa di-input: {availableStock - otherAssetOutTotal}" 
                            });
                        }

                        // Update TblTAssetOut dengan quantity baru (bukan difference)
                        var assetOut = _context.TblTAssetOuts
                            .FirstOrDefault(ao => ao.NomorAsset == asset.NomorAsset && ao.KodeBarang == asset.KodeBarang && ao.Qty == oldQty);
                        
                        if (assetOut != null)
                        {
                            var qtyawal = assetOut.Qty;
                            var qtybaru = newQty;
                            if (qtybaru > qtyawal)
                            {
                                assetIn.Qty -= qtyDifference;
                                assetIn.ModifiedAt = DateTime.Now;
                                assetIn.ModifiedBy = "system";
                            }
                            else if (qtybaru < qtyawal)
                            {
                                assetIn.Qty += qtyDifference;
                                assetIn.ModifiedAt = DateTime.Now;
                                assetIn.ModifiedBy = "system";
                            }

                            // Hitung total quantity Asset Out yang baru
                            var newTotalAssetOut = newQty;
                            assetOut.Qty = newTotalAssetOut;
                            assetOut.ModifiedAt = DateTime.Now;
                            assetOut.ModifiedBy = "system";
                        }
                    }
                    else
                    {
                        return BadRequest(new { 
                            success = false, 
                            message = "Asset In tidak ditemukan. Tidak bisa mengedit Asset Out tanpa Asset In yang valid." 
                        });
                    }
                }

                // Update quantity di TblTAssets
                asset.Qty = newQty;
                asset.ModifiedAt = DateTime.Now;
                asset.ModifiedBy = "system";

                _context.SaveChanges();
                transaction.Commit();

                var statusText = isAssetIn ? "Asset In" : isAssetOut ? "Asset Out" : "Asset";
                return Ok(new { success = true, message = $"Quantity {statusText} berhasil diupdate" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpDelete("DeleteAsset/{id}")]
        public IActionResult DeleteAsset(int id)
        {
            try
            {
                using var transaction = _context.Database.BeginTransaction();

                // First, load the asset with its related data to ensure EF tracking
                var assetWithRelated = _context.TblTAssets
                    .Include(a => a.TblRAssetSerials)
                    .Include(a => a.TblRAssetPos)
                    .FirstOrDefault(a => a.Id == id);

                if (assetWithRelated == null)
                {
                    return NotFound(new { success = false, message = "Asset tidak ditemukan" });
                }

                // Cek status asset untuk menentukan logika delete
                var isAssetIn = assetWithRelated.Status == (int)StatusAsset.In;
                var isAssetOut = assetWithRelated.Status == (int)StatusAsset.Out;

                // Delete related records to avoid foreign key constraint violations
                // Try both AssetId (business key) and Id (primary key) to handle different FK configurations
                
                var relatedSerialsByAssetId = _context.TblRAssetSerials
                    .Where(s => s.AssetId == assetWithRelated.AssetId)
                    .ToList();

                var relatedSerialsByPrimaryKey = _context.TblRAssetSerials
                    .Where(s => s.AssetId == assetWithRelated.Id)
                    .ToList();

                var relatedPosByAssetId = _context.TblRAssetPos
                    .Where(p => p.AssetId == assetWithRelated.AssetId)
                    .ToList();

                var relatedPosByPrimaryKey = _context.TblRAssetPos
                    .Where(p => p.AssetId == assetWithRelated.Id)
                    .ToList();

                // Combine and deduplicate
                var allRelatedSerials = relatedSerialsByAssetId.Union(relatedSerialsByPrimaryKey).ToList();
                var allRelatedPos = relatedPosByAssetId.Union(relatedPosByPrimaryKey).ToList();
                
                // Log what we're about to delete for debugging
                var serialCount = allRelatedSerials.Count;
                var poCount = allRelatedPos.Count;
                
                // Delete related serial numbers
                if (allRelatedSerials.Any())
                {
                    _context.TblRAssetSerials.RemoveRange(allRelatedSerials);
                }

                // Delete related PO records
                if (allRelatedPos.Any())
                {
                    _context.TblRAssetPos.RemoveRange(allRelatedPos);
                }

                // Save changes for related records first
                _context.SaveChanges();

                if (isAssetIn)
                {
                    // Delete Asset In: Kurangi quantity di TblTAssetIn (atau set ke 0)
                    var assetIn = _context.TblTAssetIns
                        .FirstOrDefault(ai => ai.NomorAsset == assetWithRelated.NomorAsset && ai.KodeBarang == assetWithRelated.KodeBarang);
                    
                    if (assetIn != null)
                    {
                        // Kurangi quantity di asset in
                        assetIn.Qty = (assetIn.Qty ?? 0) - (assetWithRelated.Qty ?? 0);
                        
                        // Jika quantity di AssetIn menjadi 0 atau kurang, set ke 0 atau hapus
                        if (assetIn.Qty <= 0)
                        {
                            // Set qty ke 0 karena asset sudah tidak tersedia
                            assetIn.Qty = 0;
                            assetIn.ModifiedAt = DateTime.Now;
                            assetIn.ModifiedBy = "system";
                        }
                        else
                        {
                            assetIn.ModifiedAt = DateTime.Now;
                            assetIn.ModifiedBy = "system";
                        }
                    }
                }
                else if (isAssetOut)
                {
                    // Delete Asset Out: Kurangi quantity di TblTAssetOut saja (tidak perlu update asset in)
                    var assetOut = _context.TblTAssetOuts
                        .FirstOrDefault(ao => ao.NomorAsset == assetWithRelated.NomorAsset && ao.KodeBarang == assetWithRelated.KodeBarang && ao.Qty == assetWithRelated.Qty);
                    
                    var assetIn = _context.TblTAssetIns
                        .FirstOrDefault(ao => ao.NomorAsset == assetWithRelated.NomorAsset && ao.KodeBarang == assetWithRelated.KodeBarang);

                    if (assetOut != null)
                    {
                        // Kurangi quantity di asset out
                        _context.TblTAssetOuts.Remove(assetOut);
                    }
                    if (assetIn != null)
                    {
                        assetIn.Qty = (assetIn.Qty ?? 0) + (assetWithRelated.Qty ?? 0);
                        assetIn.ModifiedAt = DateTime.Now;
                    }
                }

                // Delete transaksi asset yang dipilih dari TblTAssets
                _context.TblTAssets.Remove(assetWithRelated);
                _context.SaveChanges();

                transaction.Commit();

                var statusText = isAssetIn ? "Asset In" : isAssetOut ? "Asset Out" : "Asset";
                var actionText = isAssetIn ? "Stock asset berkurang/habis" : 
                                isAssetOut ? "Transaksi Asset Out dibatalkan" : "Transaksi dihapus";
                
                return Ok(new { 
                    success = true, 
                    message = $"Transaksi {statusText} berhasil dihapus. {actionText}.",
                    debug = new {
                        assetId = assetWithRelated.AssetId,
                        primaryKey = assetWithRelated.Id,
                        serialsDeleted = serialCount,
                        posDeleted = poCount,
                        assetCode = assetWithRelated.KodeBarang,
                        assetNumber = assetWithRelated.NomorAsset
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the inner exception details for better debugging
                var innerException = ex.InnerException?.Message ?? "No inner exception";
                var fullError = $"Error: {ex.Message}. Inner Exception: {innerException}";
                
                return StatusCode(500, new { 
                    success = false, 
                    message = "Terjadi kesalahan: " + ex.Message,
                    details = innerException,
                    fullError = fullError
                });
            }
        }

        [HttpDelete("DeleteAllAssetTransactions")]
        public IActionResult DeleteAllAssetTransactions([FromQuery] string kodeBarang, [FromQuery] string nomorAsset)
        {
            try
            {
                if (string.IsNullOrEmpty(kodeBarang) || string.IsNullOrEmpty(nomorAsset))
                {
                    return BadRequest(new { success = false, message = "Kode Barang dan Nomor Asset harus diisi" });
                }

                using var transaction = _context.Database.BeginTransaction();

                // Count records yang akan dihapus untuk logging
                var assetsToDelete = _context.TblTAssets
                    .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                    .ToList();

                var assetInToDelete = _context.TblTAssetIns
                    .Where(ai => ai.KodeBarang == kodeBarang && ai.NomorAsset == nomorAsset)
                    .ToList();

                var assetOutToDelete = _context.TblTAssetOuts
                    .Where(ao => ao.KodeBarang == kodeBarang && ao.NomorAsset == nomorAsset)
                    .ToList();

                if (!assetsToDelete.Any() && !assetInToDelete.Any() && !assetOutToDelete.Any())
                {
                    return NotFound(new { success = false, message = "Tidak ada data asset yang ditemukan untuk dihapus" });
                }

                // Delete dari semua table
                // First, delete related records to avoid foreign key constraint violations
                
                // 1. Delete related serial numbers for all assets
                var assetIds = assetsToDelete.Select(a => a.AssetId).ToList();
                var relatedSerials = _context.TblRAssetSerials
                    .Where(s => assetIds.Contains(s.AssetId))
                    .ToList();
                if (relatedSerials.Any())
                {
                    _context.TblRAssetSerials.RemoveRange(relatedSerials);
                }

                // 2. Delete related PO records for all assets
                var relatedPos = _context.TblRAssetPos
                    .Where(p => assetIds.Contains(p.AssetId))
                    .ToList();
                if (relatedPos.Any())
                {
                    _context.TblRAssetPos.RemoveRange(relatedPos);
                }

                // 3. Delete TblTAssets
                if (assetsToDelete.Any())
                {
                    _context.TblTAssets.RemoveRange(assetsToDelete);
                }

                // 4. Delete TblTAssetIn
                if (assetInToDelete.Any())
                {
                    _context.TblTAssetIns.RemoveRange(assetInToDelete);
                }

                // 5. Delete TblTAssetOut
                if (assetOutToDelete.Any())
                {
                    _context.TblTAssetOuts.RemoveRange(assetOutToDelete);
                }

                _context.SaveChanges();
                transaction.Commit();

                // Prepare success message dengan detail
                var deletedCounts = new List<string>();
                if (assetsToDelete.Any()) deletedCounts.Add($"{assetsToDelete.Count} transaksi asset");
                if (assetInToDelete.Any()) deletedCounts.Add($"{assetInToDelete.Count} record asset in");
                if (assetOutToDelete.Any()) deletedCounts.Add($"{assetOutToDelete.Count} record asset out");
                if (relatedSerials.Any()) deletedCounts.Add($"{relatedSerials.Count} serial numbers");
                if (relatedPos.Any()) deletedCounts.Add($"{relatedPos.Count} PO records");

                var message = $"Berhasil menghapus {string.Join(", ", deletedCounts)} untuk asset {kodeBarang}/{nomorAsset}";

                return Ok(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpGet("GetSerialNumbers/{assetId}")]
        public async Task<IActionResult> GetSerialNumbers(int assetId)
        {
            try
            {
                // Find the asset first to get the correct AssetId
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.Id == assetId);
                if (asset == null)
                {
                    return Ok(new List<object>()); // Return empty list if asset not found
                }

                var serialNumbers = await _context.TblRAssetSerials
                    .Where(s => s.AssetId == asset.AssetId)
                    .OrderBy(s => s.SerialNumber)
                    .Select(s => new
                    {
                        SerialId = s.SerialId,
                        SerialNumber = s.SerialNumber,
                        State = s.State,
                        Status = s.Status,
                        Notes = s.Notes,
                        CreatedAt = s.CreatedAt,
                        CreatedBy = s.CreatedBy
                    })
                    .ToListAsync();

                return Ok(serialNumbers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpGet("GetPoNumbers/{assetId}")]
        public async Task<IActionResult> GetPoNumbers(int assetId)
        {
            try
            {
                // Find the asset first to get the correct AssetId
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.Id == assetId);
                if (asset == null)
                {
                    return Ok(new List<object>()); // Return empty list if asset not found
                }

                var poNumbers = await _context.TblRAssetPos
                    .Where(p => p.AssetId == asset.AssetId)
                    .OrderBy(p => p.PoNumber)
                    .ThenBy(p => p.PoItem)
                    .Select(p => new
                    {
                        Id = p.Id,
                        PoNumber = p.PoNumber,
                        PoItem = p.PoItem,
                        CreatedAt = p.CreatedAt,
                        CreatedBy = p.CreatedBy
                    })
                    .ToListAsync();

                return Ok(poNumbers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpGet("GetAvailableSerials/{assetId}")]
        public async Task<IActionResult> GetAvailableSerials(int assetId)
        {
            try
            {
                // Find the asset to get its details
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.Id == assetId);
                if (asset == null)
                {
                    return Ok(new List<object>());
                }

                // For asset in edit: get all serials with status 1 for the same asset group
                // For asset out edit: get all serials with status 1 from the source asset in
                if (asset.Status == (int)StatusAsset.In)
                {
                    // Get all available serials for asset in of the same group
                    var serialNumbers = await _context.TblRAssetSerials
                        .Where(s => s.Status == 1 && 
                               _context.TblTAssets.Any(a => a.AssetId == s.AssetId && 
                                                           a.NomorAsset == asset.NomorAsset && 
                                                           a.KodeBarang == asset.KodeBarang && 
                                                           a.Status == (int)StatusAsset.In))
                        .OrderBy(s => s.SerialNumber)
                        .Select(s => new
                        {
                            SerialId = s.SerialId,
                            SerialNumber = s.SerialNumber,
                            State = s.State,
                            AssetId = s.AssetId,
                            IsCurrentlyAssigned = s.AssetId == asset.AssetId
                        })
                        .ToListAsync();

                    return Ok(serialNumbers);
                }
                else if (asset.Status == (int)StatusAsset.Out)
                {
                    // Get available serials from source asset in
                    var sourceAssetIn = await _context.TblTAssetIns
                        .FirstOrDefaultAsync(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);

                    if (sourceAssetIn != null)
                    {
                        var serialNumbers = await _context.TblRAssetSerials
                            .Where(s => s.Status == 1 && 
                                   _context.TblTAssets.Any(a => a.AssetId == s.AssetId && 
                                                               a.AssetInId == sourceAssetIn.Id && 
                                                               a.Status == (int)StatusAsset.In))
                            .OrderBy(s => s.SerialNumber)
                            .Select(s => new
                            {
                                SerialId = s.SerialId,
                                SerialNumber = s.SerialNumber,
                                State = s.State,
                                AssetId = s.AssetId,
                                IsCurrentlyAssigned = s.AssetId == asset.AssetId
                            })
                            .ToListAsync();

                        return Ok(serialNumbers);
                    }
                }

                return Ok(new List<object>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpGet("GetAvailablePos/{assetId}")]
        public async Task<IActionResult> GetAvailablePos(int assetId)
        {
            try
            {
                // Find the asset to get its details
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.Id == assetId);
                if (asset == null)
                {
                    return Ok(new List<object>());
                }

                // For asset in edit: get all POs for the same asset group
                // For asset out edit: get all POs from the source asset in
                if (asset.Status == (int)StatusAsset.In)
                {
                    // Get all POs for asset in of the same group
                    var poNumbers = await _context.TblRAssetPos
                        .Where(p => _context.TblTAssets.Any(a => a.AssetId == p.AssetId && 
                                                                a.NomorAsset == asset.NomorAsset && 
                                                                a.KodeBarang == asset.KodeBarang && 
                                                                a.Status == (int)StatusAsset.In))
                        .OrderBy(p => p.PoNumber)
                        .ThenBy(p => p.PoItem)
                        .Select(p => new
                        {
                            Id = p.Id,
                            PoNumber = p.PoNumber,
                            PoItem = p.PoItem,
                            AssetId = p.AssetId,
                            IsCurrentlyAssigned = p.AssetId == asset.AssetId
                        })
                        .ToListAsync();

                    return Ok(poNumbers);
                }
                else if (asset.Status == (int)StatusAsset.Out)
                {
                    // Get available POs from source asset in
                    var sourceAssetIn = await _context.TblTAssetIns
                        .FirstOrDefaultAsync(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);

                    if (sourceAssetIn != null)
                    {
                        var poNumbers = await _context.TblRAssetPos
                            .Where(p => _context.TblTAssets.Any(a => a.AssetId == p.AssetId && 
                                                                    a.AssetInId == sourceAssetIn.Id && 
                                                                    a.Status == (int)StatusAsset.In))
                            .OrderBy(p => p.PoNumber)
                            .ThenBy(p => p.PoItem)
                            .Select(p => new
                            {
                                Id = p.Id,
                                PoNumber = p.PoNumber,
                                PoItem = p.PoItem,
                                AssetId = p.AssetId,
                                IsCurrentlyAssigned = p.AssetId == asset.AssetId
                            })
                            .ToListAsync();

                        return Ok(poNumbers);
                    }
                }

                return Ok(new List<object>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        // ===== SERIAL NUMBER CRUD ENDPOINTS =====

        [HttpPut("UpdateSerial/{serialId}")]
        public async Task<IActionResult> UpdateSerial(int serialId, [FromBody] UpdateSerialRequest request)
        {
            try
            {
                var serial = await _context.TblRAssetSerials.FirstOrDefaultAsync(s => s.SerialId == serialId);
                if (serial == null)
                {
                    return NotFound(new { success = false, message = "Serial number tidak ditemukan" });
                }

                // Update serial data
                serial.SerialNumber = request.SerialNumber;
                serial.State = request.StateId;
                serial.Notes = request.Notes;
                serial.ModifiedAt = DateTime.Now;
                serial.ModifiedBy = "system";

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Serial number berhasil diupdate" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpDelete("DeleteSerial/{serialId}")]
        public async Task<IActionResult> DeleteSerial(int serialId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var serial = await _context.TblRAssetSerials.FirstOrDefaultAsync(s => s.SerialId == serialId);
                if (serial == null)
                {
                    return NotFound(new { success = false, message = "Serial number tidak ditemukan" });
                }

                // Find associated asset to update quantity
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.AssetId == serial.AssetId);
                if (asset != null)
                {
                    // Decrease asset quantity by 1
                    asset.Qty = Math.Max(0, (asset.Qty ?? 1) - 1);
                    asset.ModifiedAt = DateTime.Now;
                    asset.ModifiedBy = "system";

                    // Also update TblTAssetIn if it's an asset in
                    if (asset.Status == (int)StatusAsset.In)
                    {
                        var assetIn = await _context.TblTAssetIns
                            .FirstOrDefaultAsync(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);
                        if (assetIn != null)
                        {
                            assetIn.Qty = Math.Max(0, (assetIn.Qty ?? 1) - 1);
                            assetIn.ModifiedAt = DateTime.Now;
                            assetIn.ModifiedBy = "system";
                        }
                    }
                }

                // Delete serial
                _context.TblRAssetSerials.Remove(serial);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Serial number berhasil dihapus dan quantity diupdate" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpPost("AddSerial")]
        public async Task<IActionResult> AddSerial([FromBody] AddSerialRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find the asset to get AssetId
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.Id == request.AssetId);
                if (asset == null)
                {
                    return NotFound(new { success = false, message = "Asset tidak ditemukan" });
                }

                // Check if serial number already exists
                var existingSerial = await _context.TblRAssetSerials
                    .FirstOrDefaultAsync(s => s.SerialNumber == request.SerialNumber);
                if (existingSerial != null)
                {
                    return BadRequest(new { success = false, message = "Serial number sudah ada" });
                }

                // Generate new SerialId
                var maxSerialId = await _context.TblRAssetSerials.MaxAsync(s => (int?)s.SerialId) ?? 0;

                // Add new serial
                var newSerial = new TblRAssetSerial
                {
                    SerialId = maxSerialId + 1,
                    AssetId = asset.AssetId,
                    SerialNumber = request.SerialNumber,
                    State = request.StateId,
                    Notes = request.Notes,
                    Status = 1, // Active
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };

                _context.TblRAssetSerials.Add(newSerial);

                // Increase asset quantity by 1
                asset.Qty = (asset.Qty ?? 0) + 1;
                asset.ModifiedAt = DateTime.Now;
                asset.ModifiedBy = "system";

                // Also update TblTAssetIn if it's an asset in
                if (asset.Status == (int)StatusAsset.In)
                {
                    var assetIn = await _context.TblTAssetIns
                        .FirstOrDefaultAsync(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);
                    if (assetIn != null)
                    {
                        assetIn.Qty = (assetIn.Qty ?? 0) + 1;
                        assetIn.ModifiedAt = DateTime.Now;
                        assetIn.ModifiedBy = "system";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Serial number berhasil ditambahkan dan quantity diupdate" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        // ===== PO NUMBER CRUD ENDPOINTS =====

        [HttpPut("UpdatePo/{poId}")]
        public async Task<IActionResult> UpdatePo(int poId, [FromBody] UpdatePoRequest request)
        {
            try
            {
                var po = await _context.TblRAssetPos.FirstOrDefaultAsync(p => p.Id == poId);
                if (po == null)
                {
                    return NotFound(new { success = false, message = "PO number tidak ditemukan" });
                }

                // Update PO data
                po.PoNumber = request.PoNumber;
                po.PoItem = request.PoItem;
                po.ModifiedAt = DateTime.Now;
                po.ModifiedBy = "system";

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "PO number berhasil diupdate" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpDelete("DeletePo/{poId}")]
        public async Task<IActionResult> DeletePo(int poId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var po = await _context.TblRAssetPos.FirstOrDefaultAsync(p => p.Id == poId);
                if (po == null)
                {
                    return NotFound(new { success = false, message = "PO number tidak ditemukan" });
                }

                // Find associated asset to update quantity
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.AssetId == po.AssetId);
                if (asset != null)
                {
                    // Decrease asset quantity by 1
                    asset.Qty = Math.Max(0, (asset.Qty ?? 1) - 1);
                    asset.ModifiedAt = DateTime.Now;
                    asset.ModifiedBy = "system";

                    // Also update TblTAssetIn if it's an asset in
                    if (asset.Status == (int)StatusAsset.In)
                    {
                        var assetIn = await _context.TblTAssetIns
                            .FirstOrDefaultAsync(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);
                        if (assetIn != null)
                        {
                            assetIn.Qty = Math.Max(0, (assetIn.Qty ?? 1) - 1);
                            assetIn.ModifiedAt = DateTime.Now;
                            assetIn.ModifiedBy = "system";
                        }
                    }
                }

                // Delete PO
                _context.TblRAssetPos.Remove(po);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "PO number berhasil dihapus dan quantity diupdate" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpPost("AddPo")]
        public async Task<IActionResult> AddPo([FromBody] AddPoRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find the asset to get AssetId
                var asset = await _context.TblTAssets.FirstOrDefaultAsync(a => a.Id == request.AssetId);
                if (asset == null)
                {
                    return NotFound(new { success = false, message = "Asset tidak ditemukan" });
                }

                // Add new PO
                var newPo = new TblRAssetPo
                {
                    AssetId = asset.AssetId,
                    PoNumber = request.PoNumber,
                    PoItem = request.PoItem,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "system"
                };

                _context.TblRAssetPos.Add(newPo);

                // Increase asset quantity by 1
                asset.Qty = (asset.Qty ?? 0) + 1;
                asset.ModifiedAt = DateTime.Now;
                asset.ModifiedBy = "system";

                // Also update TblTAssetIn if it's an asset in
                if (asset.Status == (int)StatusAsset.In)
                {
                    var assetIn = await _context.TblTAssetIns
                        .FirstOrDefaultAsync(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);
                    if (assetIn != null)
                    {
                        assetIn.Qty = (assetIn.Qty ?? 0) + 1;
                        assetIn.ModifiedAt = DateTime.Now;
                        assetIn.ModifiedBy = "system";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "PO number berhasil ditambahkan dan quantity diupdate" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
            }
        }

        [HttpGet("GetAssetSerialNumbers")]
        public IActionResult GetAssetSerialNumbers(string kodeBarang, string nomorAsset)
        {
            try
            {
                var serials = _context.TblRAssetSerials
                    .Include(s => s.Asset)
                    .Where(s => s.Asset.KodeBarang == kodeBarang && s.Asset.NomorAsset == nomorAsset)
                    .OrderBy(s => s.SerialNumber)
                    .Select(s => new
                    {
                        Id = s.SerialId,
                        SerialNumber = s.SerialNumber
                    })
                    .ToList();

                return Ok(new { Success = true, Data = serials });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("GetPODetails")]
        public IActionResult GetPODetails(string poNumber)
        {
            try
            {
                var poDetails = _context.TblRAssetPos
                    .Where(p => p.PoNumber == poNumber)
                    .Select(p => new
                    {
                        PoNumber = p.PoNumber,
                        PoItem = p.PoItem
                    })
                    .FirstOrDefault();

                if (poDetails == null)
                {
                    return NotFound(new { Success = false, Message = "PO not found" });
                }

                return Ok(new { Success = true, Data = poDetails });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("GetAssetSerialNumbersPaginated")]
        public IActionResult GetAssetSerialNumbersPaginated(
            string kodeBarang, 
            string nomorAsset, 
            int page = 1, 
            int pageSize = 10, 
            string search = "")
        {
            try
            {
                // Debug logging
                Console.WriteLine($"GetAssetSerialNumbersPaginated called with: kodeBarang={kodeBarang}, nomorAsset={nomorAsset}");
                
                // First, try to find the asset and get its AssetId
                var asset = _context.TblTAssets
                    .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                    .FirstOrDefault();
                
                if (asset == null)
                {
                    Console.WriteLine($"No asset found with kodeBarang={kodeBarang}, nomorAsset={nomorAsset}");
                    return Ok(new { Success = true, Data = new { Items = new List<object>(), TotalItems = 0, TotalPages = 0, CurrentPage = page, PageSize = pageSize } });
                }
                
                Console.WriteLine($"Found asset with AssetId={asset.AssetId}");
                
                // Now query serials using AssetId
                var query = _context.TblRAssetSerials
                    .Where(s => s.AssetId == asset.AssetId);

                // Debug: Log the SQL query and count
                var totalBeforeFilter = query.Count();
                Console.WriteLine($"Total serials found for AssetId {asset.AssetId}: {totalBeforeFilter}");

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => 
                        s.SerialNumber.Contains(search) || 
                        (s.Notes != null && s.Notes.Contains(search)));
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var serials = query
                    .OrderBy(s => s.SerialNumber)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        Id = s.SerialId,
                        SerialNumber = s.SerialNumber,
                        Description = s.Notes ?? "",
                        Status = s.Status == 1 ? "Active" : "Inactive",
                        CreatedAt = s.CreatedAt,
                        AssetId = s.AssetId
                    })
                    .ToList();

                Console.WriteLine($"Returning {serials.Count} serials out of {totalItems} total");

                var result = new
                {
                    Items = serials,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAssetSerialNumbersPaginated: {ex.Message}");
                return BadRequest(new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet("GetPODetailsPaginated")]
        public IActionResult GetPODetailsPaginated(
            string poNumber = "", 
            string kodeBarang = "", 
            string nomorAsset = "", 
            int page = 1, 
            int pageSize = 10, 
            string search = "")
        {
            try
            {
                // Debug logging
                Console.WriteLine($"GetPODetailsPaginated called with: poNumber={poNumber}, kodeBarang={kodeBarang}, nomorAsset={nomorAsset}");
                
                var query = _context.TblRAssetPos.AsQueryable();

                // If we have asset details, find by AssetId
                if (!string.IsNullOrEmpty(kodeBarang) && !string.IsNullOrEmpty(nomorAsset))
                {
                    var asset = _context.TblTAssets
                        .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                        .FirstOrDefault();
                    
                    if (asset != null)
                    {
                        Console.WriteLine($"Found asset with AssetId={asset.AssetId}");
                        query = query.Where(p => p.AssetId == asset.AssetId);
                    }
                    else
                    {
                        Console.WriteLine($"No asset found with kodeBarang={kodeBarang}, nomorAsset={nomorAsset}");
                        return Ok(new { Success = true, Data = new { Items = new List<object>(), TotalItems = 0, TotalPages = 0, CurrentPage = page, PageSize = pageSize } });
                    }
                }
                // Filter by PO Number if provided
                else if (!string.IsNullOrEmpty(poNumber))
                {
                    query = query.Where(p => p.PoNumber == poNumber);
                }

                // Debug: Log the SQL query and count
                var totalBeforeFilter = query.Count();
                Console.WriteLine($"Total PO details found: {totalBeforeFilter}");

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => 
                        (p.PoNumber != null && p.PoNumber.Contains(search)) || 
                        (p.PoItem != null && p.PoItem.Contains(search)));
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var poDetails = query
                    .OrderBy(p => p.PoNumber)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new
                    {
                        Id = p.Id,
                        PoNumber = p.PoNumber,
                        PoItem = p.PoItem ?? "",
                        Vendor = "", // Field not available in current model
                        PoDate = p.CreatedAt,
                        AssetId = p.AssetId
                    })
                    .ToList();

                Console.WriteLine($"Returning {poDetails.Count} PO details out of {totalItems} total");

                var result = new
                {
                    Items = poDetails,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPODetailsPaginated: {ex.Message}");
                return BadRequest(new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }
        
        [HttpGet("DebugAssetData")]
        public IActionResult DebugAssetData(string kodeBarang, string nomorAsset)
        {
            try
            {
                // Get asset information
                var assets = _context.TblTAssets
                    .Where(a => a.KodeBarang == kodeBarang && a.NomorAsset == nomorAsset)
                    .Select(a => new { 
                        a.Id, 
                        a.AssetId, 
                        a.KodeBarang, 
                        a.NomorAsset, 
                        a.NamaBarang 
                    })
                    .ToList();

                // Get all serial numbers for these assets
                var assetIds = assets.Select(a => a.AssetId).ToList();
                var serials = _context.TblRAssetSerials
                    .Where(s => assetIds.Contains(s.AssetId))
                    .Select(s => new { 
                        s.SerialId, 
                        s.AssetId, 
                        s.SerialNumber, 
                        s.Status 
                    })
                    .ToList();

                // Get all PO details for these assets
                var pos = _context.TblRAssetPos
                    .Where(p => assetIds.Contains(p.AssetId))
                    .Select(p => new { 
                        p.Id, 
                        p.AssetId, 
                        p.PoNumber, 
                        p.PoItem 
                    })
                    .ToList();

                return Ok(new { 
                    Assets = assets, 
                    Serials = serials, 
                    POs = pos,
                    SearchParams = new { kodeBarang, nomorAsset }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }

    // ===== REQUEST MODELS =====

    public class UpdateSerialRequest
    {
        public required string SerialNumber { get; set; }
        public required string StateId { get; set; }
        public string? Notes { get; set; }
    }

    public class AddSerialRequest
    {
        public int AssetId { get; set; }
        public required string SerialNumber { get; set; }
        public required string StateId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdatePoRequest
    {
        public required string PoNumber { get; set; }
        public string? PoItem { get; set; }
    }

    public class AddPoRequest
    {
        public int AssetId { get; set; }
        public required string PoNumber { get; set; }
        public string? PoItem { get; set; }
    }

    public class UpdateAssetFromReviewRequest
    {
        public int Qty { get; set; }
        public List<int>? SelectedSerials { get; set; }
        public List<int>? SelectedPos { get; set; }
        public string? State { get; set; }
        public string? PoNumber { get; set; }
        public string? PoItem { get; set; }
    }
}