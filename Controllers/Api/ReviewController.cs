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
                    .GroupBy(a => new { a.KodeBarang, a.NomorAsset })
                    .Select(g => new
                    {
                        KodeBarang = g.Key.KodeBarang,
                        NomorAsset = g.Key.NomorAsset,
                        NamaBarang = g.First().NamaBarang,
                        KategoriBarang = g.First().KategoriBarang,
                        Status = g.First().Status,
                        StatusText = g.First().Status == 1 ? "Asset In" : g.First().Status == 2 ? "Asset Out" : "Unknown",
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
                    
                    // Update qty di TblMAssetIn
                    var assetIn = _context.TblMAssetIns
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
                    var assetIn = _context.TblMAssetIns
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

                        // Update TblMAssetOut dengan quantity baru (bukan difference)
                        var assetOut = _context.TblMAssetOuts
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
                var asset = _context.TblTAssets.FirstOrDefault(a => a.Id == id);
                if (asset == null)
                {
                    return NotFound(new { success = false, message = "Asset tidak ditemukan" });
                }

                using var transaction = _context.Database.BeginTransaction();

                // Cek status asset untuk menentukan logika delete
                var isAssetIn = asset.Status == (int)StatusAsset.In;
                var isAssetOut = asset.Status == (int)StatusAsset.Out;

                if (isAssetIn)
                {
                    // Delete Asset In: Kurangi quantity di TblMAssetIn (atau set ke 0)
                    var assetIn = _context.TblMAssetIns
                        .FirstOrDefault(ai => ai.NomorAsset == asset.NomorAsset && ai.KodeBarang == asset.KodeBarang);
                    
                    if (assetIn != null)
                    {
                        // Kurangi quantity di asset in
                        assetIn.Qty = (assetIn.Qty ?? 0) - (asset.Qty ?? 0);
                        
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
                    // Delete Asset Out: Kurangi quantity di TblMAssetOut saja (tidak perlu update asset in)
                    var assetOut = _context.TblMAssetOuts
                        .FirstOrDefault(ao => ao.NomorAsset == asset.NomorAsset && ao.KodeBarang == asset.KodeBarang && ao.Qty == asset.Qty);
                    
                    var assetIn = _context.TblMAssetIns
                        .FirstOrDefault(ao => ao.NomorAsset == asset.NomorAsset && ao.KodeBarang == asset.KodeBarang);

                    if (assetOut != null)
                    {
                        // Kurangi quantity di asset out
                        _context.TblMAssetOuts.Remove(assetOut);
                    }
                    if (assetIn != null)
                    {
                        assetIn.Qty = (assetIn.Qty ?? 0) + (asset.Qty ?? 0);
                        assetIn.ModifiedAt = DateTime.Now;
                    }
                }

                // Delete transaksi asset yang dipilih dari TblTAssets
                _context.TblTAssets.Remove(asset);
                _context.SaveChanges();

                transaction.Commit();

                var statusText = isAssetIn ? "Asset In" : isAssetOut ? "Asset Out" : "Asset";
                var actionText = isAssetIn ? "Stock asset berkurang/habis" : 
                                isAssetOut ? "Transaksi Asset Out dibatalkan" : "Transaksi dihapus";
                
                return Ok(new { success = true, message = $"Transaksi {statusText} berhasil dihapus. {actionText}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Terjadi kesalahan: " + ex.Message });
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

                var assetInToDelete = _context.TblMAssetIns
                    .Where(ai => ai.KodeBarang == kodeBarang && ai.NomorAsset == nomorAsset)
                    .ToList();

                var assetOutToDelete = _context.TblMAssetOuts
                    .Where(ao => ao.KodeBarang == kodeBarang && ao.NomorAsset == nomorAsset)
                    .ToList();

                if (!assetsToDelete.Any() && !assetInToDelete.Any() && !assetOutToDelete.Any())
                {
                    return NotFound(new { success = false, message = "Tidak ada data asset yang ditemukan untuk dihapus" });
                }

                // Delete dari semua table
                // 1. Delete TblTAssets
                if (assetsToDelete.Any())
                {
                    _context.TblTAssets.RemoveRange(assetsToDelete);
                }

                // 2. Delete TblMAssetIn
                if (assetInToDelete.Any())
                {
                    _context.TblMAssetIns.RemoveRange(assetInToDelete);
                }

                // 3. Delete TblMAssetOut
                if (assetOutToDelete.Any())
                {
                    _context.TblMAssetOuts.RemoveRange(assetOutToDelete);
                }

                _context.SaveChanges();
                transaction.Commit();

                // Prepare success message dengan detail
                var deletedCounts = new List<string>();
                if (assetsToDelete.Any()) deletedCounts.Add($"{assetsToDelete.Count} transaksi asset");
                if (assetInToDelete.Any()) deletedCounts.Add($"{assetInToDelete.Count} record asset in");
                if (assetOutToDelete.Any()) deletedCounts.Add($"{assetOutToDelete.Count} record asset out");

                var message = $"Berhasil menghapus {string.Join(", ", deletedCounts)} untuk asset {kodeBarang}/{nomorAsset}";

                return Ok(new { success = true, message = message });
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