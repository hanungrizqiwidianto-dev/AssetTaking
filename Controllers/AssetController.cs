using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using ClosedXML.Excel;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AssetTaking.Controllers
{
    public class AssetController : Controller  
    {
        private readonly DbRndAssetTakingContext _context;

        public AssetController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");
            return View();
        }

        public async Task<IActionResult> AssetIn()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");
            
            // Get categories from database
            var categories = await _context.TblMAssetCategories
                .Where(x => !string.IsNullOrEmpty(x.KategoriBarang))
                .OrderBy(x => x.KategoriBarang)
                .ToListAsync();
            
            ViewBag.Categories = categories;
            return View();
        }

        public async Task<IActionResult> AssetOut()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");
            
            // Get categories from database
            var categories = await _context.TblMAssetCategories
                .Where(x => !string.IsNullOrEmpty(x.KategoriBarang))
                .OrderBy(x => x.KategoriBarang)
                .ToListAsync();
            
            ViewBag.Categories = categories;
            return View();
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Asset In Template");
                    
                    // Header columns
                    worksheet.Cell(1, 1).Value = "Nama Barang";
                    worksheet.Cell(1, 2).Value = "Nomor Asset";
                    worksheet.Cell(1, 3).Value = "Kode Barang";
                    worksheet.Cell(1, 4).Value = "Kategori Barang";
                    worksheet.Cell(1, 5).Value = "Tanggal Masuk";
                    worksheet.Cell(1, 6).Value = "Qty";
                    
                    // Format header
                    var headerRange = worksheet.Range(1, 1, 1, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    
                    // Add sample data
                    worksheet.Cell(2, 1).Value = "Laptop Dell Inspiron";
                    worksheet.Cell(2, 2).Value = "AST001";
                    worksheet.Cell(2, 3).Value = "LPT001";
                    worksheet.Cell(2, 4).Value = "Elektronik";
                    worksheet.Cell(2, 5).Value = DateTime.Now.ToString("dd/MM/yyyy");
                    worksheet.Cell(2, 6).Value = 1;
                    
                    worksheet.Cell(3, 1).Value = "Monitor Samsung";
                    worksheet.Cell(3, 2).Value = "AST002";
                    worksheet.Cell(3, 3).Value = "MON001";
                    worksheet.Cell(3, 4).Value = "Elektronik";
                    worksheet.Cell(3, 5).Value = DateTime.Now.ToString("dd/MM/yyyy");
                    worksheet.Cell(3, 6).Value = 2;
                    
                    // Auto fit columns
                    worksheet.ColumnsUsed().AdjustToContents();
                    
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Asset_In.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Terjadi kesalahan saat membuat template: {ex.Message}";
                return RedirectToAction("AssetIn");
            }
        }

        [HttpGet]
        public IActionResult DownloadTemplateAssetOut()
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Asset Out Template");
                    
                    // Header columns
                    worksheet.Cell(1, 1).Value = "Nomor Asset";
                    worksheet.Cell(1, 2).Value = "Kode Barang";
                    worksheet.Cell(1, 3).Value = "Qty";
                    worksheet.Cell(1, 4).Value = "Tanggal Keluar";
                    
                    // Format header
                    var headerRange = worksheet.Range(1, 1, 1, 4);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    
                    // Add sample data
                    worksheet.Cell(2, 1).Value = "AST001";
                    worksheet.Cell(2, 2).Value = "LPT001";
                    worksheet.Cell(2, 3).Value = 1;
                    worksheet.Cell(2, 4).Value = DateTime.Now.ToString("dd/MM/yyyy");
                    
                    worksheet.Cell(3, 1).Value = "AST002";
                    worksheet.Cell(3, 2).Value = "MON001";
                    worksheet.Cell(3, 3).Value = 2;
                    worksheet.Cell(3, 4).Value = DateTime.Now.ToString("dd/MM/yyyy");
                    
                    // // Add instruction note
                    // worksheet.Cell(5, 1).Value = "Catatan:";
                    // worksheet.Cell(6, 1).Value = "- Nomor Asset dan Kode Barang harus sesuai dengan data yang ada di sistem";
                    // worksheet.Cell(7, 1).Value = "- Qty yang diinput akan mengurangi stok yang tersedia";
                    // worksheet.Cell(8, 1).Value = "- Pastikan stok tersedia mencukupi untuk quantity yang diinput";
                    // worksheet.Cell(9, 1).Value = "- Format tanggal: dd/MM/yyyy (contoh: 12/10/2025)";
                    
                    // // Style instruction note
                    // var noteRange = worksheet.Range(5, 1, 9, 1);
                    // noteRange.Style.Font.Italic = true;
                    // noteRange.Style.Font.FontColor = XLColor.DarkBlue;
                    
                    // Auto fit columns
                    worksheet.ColumnsUsed().AdjustToContents();
                    
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Template_Asset_Out.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Terjadi kesalahan saat membuat template: {ex.Message}";
                return RedirectToAction("AssetOut");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Silakan pilih file Excel untuk diupload.";
                return RedirectToAction("AssetIn");
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                TempData["Error"] = "File harus berformat Excel (.xlsx atau .xls).";
                return RedirectToAction("AssetIn");
            }

            var errorMessages = new List<string>();
            var successCount = 0;
            var duplicateCheck = new HashSet<string>();
            var currentUser = HttpContext.Session.GetString("Nrp");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                        if (rowCount <= 1)
                        {
                            TempData["Error"] = "File Excel tidak memiliki data atau hanya berisi header.";
                            return RedirectToAction("AssetIn");
                        }

                        // Validasi dan import data
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var namaBarang = worksheet.Cell(row, 1).GetString().Trim();
                            var nomorAsset = worksheet.Cell(row, 2).GetString().Trim();
                            var kodeBarang = worksheet.Cell(row, 3).GetString().Trim();
                            var kategoriBarang = worksheet.Cell(row, 4).GetString().Trim();
                            var tanggalMasukText = worksheet.Cell(row, 5).GetString().Trim();
                            var qtyText = worksheet.Cell(row, 6).GetString().Trim();

                            // Validasi required fields
                            if (string.IsNullOrEmpty(namaBarang))
                            {
                                errorMessages.Add($"Baris {row}: Nama Barang tidak boleh kosong.");
                                continue;
                            }

                            if (string.IsNullOrEmpty(nomorAsset))
                            {
                                errorMessages.Add($"Baris {row}: Nomor Asset tidak boleh kosong.");
                                continue;
                            }

                            if (string.IsNullOrEmpty(kodeBarang))
                            {
                                errorMessages.Add($"Baris {row}: Kode Barang tidak boleh kosong.");
                                continue;
                            }

                            // Validasi dan parse tanggal masuk
                            DateTime tanggalMasuk;
                            if (string.IsNullOrEmpty(tanggalMasukText))
                            {
                                // Jika tanggal masuk kosong, gunakan tanggal hari ini
                                tanggalMasuk = DateTime.Now.Date;
                            }
                            else
                            {
                                // Coba parse tanggal dengan berbagai format
                                string[] dateFormats = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
                                if (!DateTime.TryParseExact(tanggalMasukText, dateFormats, null, System.Globalization.DateTimeStyles.None, out tanggalMasuk))
                                {
                                    if (!DateTime.TryParse(tanggalMasukText, out tanggalMasuk))
                                    {
                                        errorMessages.Add($"Baris {row}: Format Tanggal Masuk tidak valid. Gunakan format dd/MM/yyyy (contoh: 12/10/2025).");
                                        continue;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(qtyText) || !int.TryParse(qtyText, out int qty) || qty <= 0)
                            {
                                errorMessages.Add($"Baris {row}: Qty harus diisi dengan angka yang valid dan lebih dari 0.");
                                continue;
                            }

                            // Validasi duplikasi dalam file Excel
                            var uniqueKey = $"{kodeBarang}_{nomorAsset}";
                            if (duplicateCheck.Contains(uniqueKey))
                            {
                                errorMessages.Add($"Baris {row}: Kombinasi Kode Barang '{kodeBarang}' dan Nomor Asset '{nomorAsset}' sudah ada dalam file Excel.");
                                continue;
                            }
                            duplicateCheck.Add(uniqueKey);

                            // Check if asset already exists in database
                            var existingAsset = await _context.TblTAssets
                                .Where(x => x.KodeBarang == kodeBarang && x.NomorAsset == nomorAsset)
                                .FirstOrDefaultAsync();

                            if (existingAsset != null)
                            {
                                // Update existing asset quantity
                                existingAsset.Qty = (existingAsset.Qty ?? 0) + qty;
                                existingAsset.ModifiedAt = DateTime.Now;
                                existingAsset.ModifiedBy = currentUser;
                                
                                // Also update the asset in record if it exists
                                var existingAssetIn = await _context.TblTAssetIns
                                    .Where(x => x.KodeBarang == kodeBarang && x.NomorAsset == nomorAsset)
                                    .FirstOrDefaultAsync();
                                
                                if (existingAssetIn != null)
                                {
                                    existingAssetIn.Qty = (existingAssetIn.Qty ?? 0) + qty;
                                    existingAssetIn.ModifiedAt = DateTime.Now;
                                    existingAssetIn.ModifiedBy = currentUser;
                                }
                                else
                                {
                                    // Create new asset in record
                                    var assetIn = new TblTAssetIn
                                    {
                                        NamaBarang = namaBarang,
                                        NomorAsset = nomorAsset,
                                        KodeBarang = kodeBarang,
                                        KategoriBarang = kategoriBarang,
                                        Qty = qty,
                                        CreatedAt = DateTime.Now,
                                        CreatedBy = currentUser
                                    };
                                    _context.TblTAssetIns.Add(assetIn);
                                }
                            }
                            else
                            {
                                // Create new asset and asset in records
                                var assetIn = new TblTAssetIn
                                {
                                    NamaBarang = namaBarang,
                                    NomorAsset = nomorAsset,
                                    KodeBarang = kodeBarang,
                                    KategoriBarang = kategoriBarang,
                                    Qty = qty,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = currentUser
                                };

                                var asset = new TblTAsset
                                {
                                    NamaBarang = namaBarang,
                                    NomorAsset = nomorAsset,
                                    KodeBarang = kodeBarang,
                                    KategoriBarang = kategoriBarang,
                                    TanggalMasuk = tanggalMasuk,
                                    Qty = qty,
                                    Status = 1, // Status 1 = Asset In
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = currentUser
                                };

                                _context.TblTAssetIns.Add(assetIn);
                                _context.TblTAssets.Add(asset);
                            }

                            successCount++;
                        }

                        if (successCount > 0)
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // Buat pesan hasil
                var resultMessage = new StringBuilder();
                if (successCount > 0)
                {
                    resultMessage.AppendLine($"Berhasil mengimpor {successCount} data asset.");
                }

                if (errorMessages.Any())
                {
                    resultMessage.AppendLine($"Ditemukan {errorMessages.Count} error:");
                    foreach (var error in errorMessages.Take(10)) // Batasi tampilan error maksimal 10
                    {
                        resultMessage.AppendLine($"- {error}");
                    }
                    if (errorMessages.Count > 10)
                    {
                        resultMessage.AppendLine($"... dan {errorMessages.Count - 10} error lainnya.");
                    }
                }

                if (errorMessages.Any())
                {
                    var hasSuccess = successCount > 0;
                    var status = hasSuccess ? "warning" : "error";
                    var title = hasSuccess ? "Import Selesai dengan Peringatan" : "Import Gagal";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new 
                        { 
                            success = hasSuccess,
                            status = status,
                            title = title,
                            message = resultMessage.ToString(),
                            successCount = successCount,
                            errorCount = errorMessages.Count
                        });
                    }
                    
                    TempData["Warning"] = resultMessage.ToString();
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new 
                        { 
                            success = true,
                            status = "success",
                            title = "Import Berhasil!",
                            message = resultMessage.ToString(),
                            successCount = successCount,
                            errorCount = 0
                        });
                    }
                    
                    TempData["Success"] = resultMessage.ToString();
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new 
                    { 
                        success = false,
                        status = "error",
                        title = "Error!",
                        message = $"Terjadi kesalahan saat memproses file: {ex.Message}",
                        successCount = 0,
                        errorCount = 1
                    });
                }
                
                TempData["Error"] = $"Terjadi kesalahan saat memproses file: {ex.Message}";
            }

            return RedirectToAction("AssetIn");
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcelAssetOut(IFormFile file)
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Silakan pilih file Excel untuk diupload.";
                return RedirectToAction("AssetOut");
            }

            if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
            {
                TempData["Error"] = "File harus berformat Excel (.xlsx atau .xls).";
                return RedirectToAction("AssetOut");
            }

            var errorMessages = new List<string>();
            var successCount = 0;
            var currentUser = HttpContext.Session.GetString("Nrp");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rowCount = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                        if (rowCount <= 1)
                        {
                            TempData["Error"] = "File Excel tidak memiliki data atau hanya berisi header.";
                            return RedirectToAction("AssetOut");
                        }

                        // Validasi dan import data
                        for (int row = 2; row <= rowCount; row++)
                        {
                            var nomorAsset = worksheet.Cell(row, 1).GetString().Trim();
                            var kodeBarang = worksheet.Cell(row, 2).GetString().Trim();
                            var qtyText = worksheet.Cell(row, 3).GetString().Trim();
                            var tanggalKeluarText = worksheet.Cell(row, 4).GetString().Trim();

                            // Validasi required fields
                            if (string.IsNullOrEmpty(nomorAsset))
                            {
                                errorMessages.Add($"Baris {row}: Nomor Asset tidak boleh kosong.");
                                continue;
                            }

                            if (string.IsNullOrEmpty(kodeBarang))
                            {
                                errorMessages.Add($"Baris {row}: Kode Barang tidak boleh kosong.");
                                continue;
                            }

                            if (string.IsNullOrEmpty(qtyText) || !int.TryParse(qtyText, out int qty) || qty <= 0)
                            {
                                errorMessages.Add($"Baris {row}: Qty harus diisi dengan angka yang valid dan lebih dari 0.");
                                continue;
                            }

                            // Validasi dan parse tanggal keluar
                            DateTime tanggalKeluar;
                            if (string.IsNullOrEmpty(tanggalKeluarText))
                            {
                                // Jika tanggal keluar kosong, gunakan tanggal hari ini
                                tanggalKeluar = DateTime.Now.Date;
                            }
                            else
                            {
                                // Coba parse tanggal dengan berbagai format
                                string[] dateFormats = { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
                                if (!DateTime.TryParseExact(tanggalKeluarText, dateFormats, null, System.Globalization.DateTimeStyles.None, out tanggalKeluar))
                                {
                                    if (!DateTime.TryParse(tanggalKeluarText, out tanggalKeluar))
                                    {
                                        errorMessages.Add($"Baris {row}: Format Tanggal Keluar tidak valid. Gunakan format dd/MM/yyyy (contoh: 12/10/2025).");
                                        continue;
                                    }
                                }
                            }

                            // Cari asset yang akan dikurangi qty nya
                            var sourceAsset = await _context.TblTAssetIns
                                .Where(x => x.NomorAsset == nomorAsset && x.KodeBarang == kodeBarang)
                                .FirstOrDefaultAsync();

                            if (sourceAsset == null)
                            {
                                errorMessages.Add($"Baris {row}: Asset dengan Nomor Asset '{nomorAsset}' dan Kode Barang '{kodeBarang}' tidak ditemukan di database.");
                                continue;
                            }

                            if (sourceAsset.Qty < qty)
                            {
                                errorMessages.Add($"Baris {row}: Qty tidak mencukupi. Stok tersedia untuk asset '{nomorAsset}' - '{kodeBarang}': {sourceAsset.Qty}, yang diminta: {qty}.");
                                continue;
                            }

                            // Buat record Asset Out
                            var assetOut = new TblTAssetOut
                            {
                                NamaBarang = sourceAsset.NamaBarang,
                                NomorAsset = sourceAsset.NomorAsset,
                                KodeBarang = sourceAsset.KodeBarang,
                                KategoriBarang = sourceAsset.KategoriBarang,
                                Qty = qty,
                                Foto = sourceAsset.Foto,
                                CreatedAt = DateTime.Now,
                                CreatedBy = currentUser
                            };

                            // Buat record Asset dengan status Out
                            var asset = new TblTAsset
                            {
                                NamaBarang = sourceAsset.NamaBarang,
                                TanggalMasuk = tanggalKeluar, // Tanggal keluar untuk asset out
                                NomorAsset = sourceAsset.NomorAsset,
                                KodeBarang = sourceAsset.KodeBarang,
                                KategoriBarang = sourceAsset.KategoriBarang,
                                Qty = qty,
                                Foto = sourceAsset.Foto,
                                Status = 2, // Status 2 = Asset Out
                                CreatedAt = DateTime.Now,
                                CreatedBy = currentUser
                            };

                            // Update qty di TblTAssetIn (kurangi stok)
                            sourceAsset.Qty -= qty;
                            sourceAsset.ModifiedAt = DateTime.Now;
                            sourceAsset.ModifiedBy = currentUser;

                            _context.TblTAssetOuts.Add(assetOut);
                            _context.TblTAssets.Add(asset);
                            successCount++;
                        }

                        if (successCount > 0)
                        {
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // Buat pesan hasil
                var resultMessage = new StringBuilder();
                if (successCount > 0)
                {
                    resultMessage.AppendLine($"Berhasil memproses {successCount} data asset out.");
                }

                if (errorMessages.Any())
                {
                    resultMessage.AppendLine($"Ditemukan {errorMessages.Count} error:");
                    foreach (var error in errorMessages.Take(10)) // Batasi tampilan error maksimal 10
                    {
                        resultMessage.AppendLine($"- {error}");
                    }
                    if (errorMessages.Count > 10)
                    {
                        resultMessage.AppendLine($"... dan {errorMessages.Count - 10} error lainnya.");
                    }
                }

                if (errorMessages.Any())
                {
                    var hasSuccess = successCount > 0;
                    var status = hasSuccess ? "warning" : "error";
                    var title = hasSuccess ? "Import Selesai dengan Peringatan" : "Import Gagal";
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new 
                        { 
                            success = hasSuccess,
                            status = status,
                            title = title,
                            message = resultMessage.ToString(),
                            successCount = successCount,
                            errorCount = errorMessages.Count
                        });
                    }
                    
                    TempData["Warning"] = resultMessage.ToString();
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new 
                        { 
                            success = true,
                            status = "success",
                            title = "Import Berhasil!",
                            message = resultMessage.ToString(),
                            successCount = successCount,
                            errorCount = 0
                        });
                    }
                    
                    TempData["Success"] = resultMessage.ToString();
                }
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new 
                    { 
                        success = false,
                        status = "error",
                        title = "Error!",
                        message = $"Terjadi kesalahan saat memproses file: {ex.Message}",
                        successCount = 0,
                        errorCount = 1
                    });
                }
                
                TempData["Error"] = $"Terjadi kesalahan saat memproses file: {ex.Message}";
            }

            return RedirectToAction("AssetOut");
        }

        public async Task<IActionResult> GenerateQR(string nama = "", string nomor = "", string kode = "", string kategori = "", int? qty = null)
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");
            
            // Get categories from database
            var categories = await _context.TblMAssetCategories
                .Where(x => !string.IsNullOrEmpty(x.KategoriBarang))
                .OrderBy(x => x.KategoriBarang)
                .ToListAsync();
            
            ViewBag.Categories = categories;
            
            // Pass data to view for form pre-population
            ViewBag.PreNamaBarang = nama;
            ViewBag.PreNomorAsset = nomor;
            ViewBag.PreKodeBarang = kode;
            ViewBag.PreKategoriBarang = kategori;
            ViewBag.PreQty = qty ?? 1;
            
            return View();
        }

        [HttpPost]
        [ActionName("GenerateQR")]
        public async Task<IActionResult> GenerateQRPost(string namaBarang, string nomorAsset, string kategoriBarang, string kodeBarang, int? qty, string generateMode = "single")
        {
            try
            {
                // Get categories for dropdown reload
                var categories = await _context.TblMAssetCategories
                    .Where(x => !string.IsNullOrEmpty(x.KategoriBarang))
                    .OrderBy(x => x.KategoriBarang)
                    .ToListAsync();
                ViewBag.Categories = categories;

                if (string.IsNullOrEmpty(namaBarang))
                {
                    ViewBag.Error = "Nama barang harus diisi";
                    return View();
                }

                if (string.IsNullOrEmpty(kategoriBarang))
                {
                    ViewBag.Error = "Kategori barang harus dipilih";
                    return View();
                }

                if (qty == null || qty <= 0)
                {
                    ViewBag.Error = "Quantity harus diisi dengan nilai yang valid (lebih dari 0)";
                    return View();
                }

                if (generateMode == "batch")
                {
                    if (qty < 2)
                    {
                        ViewBag.Error = "Untuk batch generate, quantity minimal 2";
                        return View();
                    }

                    if (qty > 100)
                    {
                        ViewBag.Error = "Untuk batch generate, quantity maksimal 100";
                        return View();
                    }

                    // Generate batch QR codes
                    var batchData = new List<Dictionary<string, object>>();
                    
                    // Get base next number for this category
                    string prefix = GetCategoryPrefix(kategoriBarang);
                    var existingCodes = await _context.TblTAssets
                        .Where(a => a.KodeBarang != null && a.KodeBarang.StartsWith(prefix + "-"))
                        .Select(a => a.KodeBarang)
                        .ToListAsync();

                    int baseNextNumber = 1;
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
                            baseNextNumber = numbers.Max() + 1;
                        }
                    }
                    
                    for (int i = 1; i <= qty; i++)
                    {
                        string uniqueKodeBarang = $"{prefix}-{baseNextNumber + i - 1}";
                        string uniqueNomorAsset = !string.IsNullOrEmpty(nomorAsset) ? $"{nomorAsset}-{i:D3}" : "";
                        
                        var qrDataObject = new Dictionary<string, object>
                        {
                            ["nomorAsset"] = uniqueNomorAsset,
                            ["namaBarang"] = namaBarang,
                            ["kodeBarang"] = uniqueKodeBarang,
                            ["kategoriBarang"] = kategoriBarang,
                            ["qty"] = 1,
                            ["batchNumber"] = i,
                            ["totalBatch"] = qty.Value
                        };

                        batchData.Add(qrDataObject);
                    }

                    ViewBag.IsBatch = true;
                    ViewBag.BatchData = batchData;
                    ViewBag.NamaBarang = namaBarang;
                    ViewBag.NomorAsset = nomorAsset;
                    ViewBag.KategoriBarang = kategoriBarang;
                    ViewBag.KodeBarang = GetCategoryPrefix(kategoriBarang);
                    ViewBag.Qty = qty;
                    ViewBag.Success = $"Batch QR Code berhasil digenerate untuk {qty} item!";

                    return View();
                }
                else
                {
                    // Single generate mode
                    if (qty != 1)
                    {
                        ViewBag.Error = "Untuk single generate, quantity harus 1";
                        return View();
                    }

                    // Generate kode barang if not provided
                    if (string.IsNullOrEmpty(kodeBarang))
                    {
                        kodeBarang = GenerateKodeBarang(kategoriBarang);
                    }

                    // Buat JSON data untuk QR Code sesuai format scan yang sudah ada
                    var qrDataObject = new
                    {
                        nomorAsset = nomorAsset ?? "",
                        namaBarang = namaBarang,
                        kodeBarang = kodeBarang,
                        kategoriBarang = kategoriBarang,
                        qty = qty.Value
                    };
                    
                    var qrData = System.Text.Json.JsonSerializer.Serialize(qrDataObject);

                    ViewBag.IsBatch = false;
                    ViewBag.NamaBarang = namaBarang;
                    ViewBag.NomorAsset = nomorAsset;
                    ViewBag.KategoriBarang = kategoriBarang;
                    ViewBag.KodeBarang = kodeBarang;
                    ViewBag.Qty = qty;
                    ViewBag.QRData = qrData;
                    ViewBag.Success = "QR Code berhasil digenerate!";

                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error generating QR Code: {ex.Message}";
                return View();
            }
        }

        private string GetCategoryPrefix(string kategori)
        {
            switch (kategori.ToUpper())
            {
                case "RND":
                case "R&D":
                case "RESEARCH AND DEVELOPMENT":
                    return "RND";
                case "SPARE PART":
                case "SPAREPART":
                case "SP":
                    return "SPA";
                case "ELEKTRONIK":
                case "ELECTRONIC":
                case "ELK":
                    return "ELK";
                case "FURNITURE":
                case "FURNITUR":
                case "FRN":
                    return "FRN";
                case "KENDARAAN":
                case "VEHICLE":
                case "VHC":
                    return "VHC";
                case "PERALATAN":
                case "EQUIPMENT":
                case "EQP":
                    return "EQP";
                default:
                    return kategori.Length >= 3 
                        ? kategori.Substring(0, 3).ToUpper() 
                        : kategori.ToUpper();
            }
        }

        private string GenerateKodeBarang(string kategori)
        {
            string prefix = GetCategoryPrefix(kategori);

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

        [HttpGet]
        public IActionResult PrintLabel(string namaBarang, string nomorAsset, string kategoriBarang, string kodeBarang, int? qty, string qrData, string batchData)
        {
            ViewBag.NamaBarang = namaBarang;
            ViewBag.NomorAsset = nomorAsset;
            ViewBag.KategoriBarang = kategoriBarang;
            ViewBag.KodeBarang = kodeBarang;
            ViewBag.Qty = qty;
            ViewBag.QRData = qrData;

            // Handle batch data
            if (!string.IsNullOrEmpty(batchData))
            {
                try
                {
                    // Deserialize as List<Dictionary<string, object>> untuk handling yang lebih baik
                    var batchItems = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(batchData);
                    ViewBag.BatchData = batchItems;
                    ViewBag.IsBatch = true;
                }
                catch (Exception ex)
                {
                    // Jika error, coba deserialize sebagai List<object> seperti sebelumnya
                    try
                    {
                        var batchItems = System.Text.Json.JsonSerializer.Deserialize<List<object>>(batchData);
                        ViewBag.BatchData = batchItems;
                        ViewBag.IsBatch = true;
                    }
                    catch (Exception ex2)
                    {
                        ViewBag.Error = $"Error parsing batch data: {ex2.Message}";
                        ViewBag.IsBatch = false;
                    }
                }
            }
            else
            {
                ViewBag.IsBatch = false;
            }

            return View();
        }
    }
}
