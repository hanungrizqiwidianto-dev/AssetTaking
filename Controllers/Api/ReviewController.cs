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
                    .Include(s => s.State)
                    .Where(s => s.AssetId == asset.AssetId)
                    .OrderBy(s => s.SerialNumber)
                    .Select(s => new
                    {
                        SerialId = s.SerialId,
                        SerialNumber = s.SerialNumber,
                        StateId = s.StateId,
                        StateName = s.State != null ? s.State.State : null,
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
    }

    public class UpdateAssetFromReviewRequest
    {
        public int Qty { get; set; }
    }
}