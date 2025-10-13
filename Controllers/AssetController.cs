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

        public IActionResult AssetIn()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");
            return View();
        }

        public IActionResult AssetOut()
        {
            if (HttpContext.Session.GetString("Nrp") == null)
                return RedirectToAction("Index", "Login");
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

                            // Validasi duplikasi dengan database
                            var existingAsset = await _context.TblMAssetIns
                                .Where(x => x.KodeBarang == kodeBarang && x.NomorAsset == nomorAsset)
                                .FirstOrDefaultAsync();

                            if (existingAsset != null)
                            {
                                errorMessages.Add($"Baris {row}: Kombinasi Kode Barang '{kodeBarang}' dan Nomor Asset '{nomorAsset}' sudah ada dalam database.");
                                continue;
                            }

                            // Jika validasi lolos, simpan ke database
                            var assetIn = new TblMAssetIn
                            {
                                NamaBarang = namaBarang,
                                NomorAsset = nomorAsset,
                                KodeBarang = kodeBarang,
                                KategoriBarang = kategoriBarang,
                                Qty = qty,
                                CreatedAt = DateTime.Now,
                                CreatedBy = currentUser
                            };

                            // Simpan ke TblTAsset juga
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

                            _context.TblMAssetIns.Add(assetIn);
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
    }
}
