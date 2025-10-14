using Microsoft.AspNetCore.Mvc;
using AssetTaking.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AssetTaking.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly DbRndAssetTakingContext _context;

        public CategoryController(DbRndAssetTakingContext context)
        {
            _context = context;
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.TblMAssetCategories
                    .OrderBy(x => x.KategoriBarang)
                    .ToListAsync();

                var result = categories.Select(category => new
                {
                    Id = category.Id,
                    KategoriBarang = category.KategoriBarang,
                    CreatedAt = category.CreatedAt?.ToString("dd/MM/yyyy HH:mm"),
                    CreatedBy = category.CreatedBy,
                    ModifiedAt = category.ModifiedAt?.ToString("dd/MM/yyyy HH:mm"),
                    ModifiedBy = category.ModifiedBy
                }).ToList();

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetCategoryList")]
        public async Task<IActionResult> GetCategoryList()
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

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
        {
            try
            {
                // Check if category already exists
                var existingCategory = await _context.TblMAssetCategories
                    .FirstOrDefaultAsync(x => x.KategoriBarang == request.KategoriBarang);

                if (existingCategory != null)
                {
                    return BadRequest(new { success = false, message = "Kategori barang sudah ada!" });
                }

                var currentUser = HttpContext.Session.GetString("Nrp") ?? "system";

                var category = new TblMAssetCategory
                {
                    KategoriBarang = request.KategoriBarang,
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUser,
                    ModifiedAt = DateTime.Now,
                    ModifiedBy = currentUser
                };

                _context.TblMAssetCategories.Add(category);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Kategori berhasil ditambahkan!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("UpdateCategory/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequest request)
        {
            try
            {
                var category = await _context.TblMAssetCategories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Kategori tidak ditemukan!" });
                }

                // Check if category name already exists (excluding current record)
                var existingCategory = await _context.TblMAssetCategories
                    .FirstOrDefaultAsync(x => x.KategoriBarang == request.KategoriBarang && x.Id != id);

                if (existingCategory != null)
                {
                    return BadRequest(new { success = false, message = "Kategori barang sudah ada!" });
                }

                var currentUser = HttpContext.Session.GetString("Nrp") ?? "system";

                category.KategoriBarang = request.KategoriBarang;
                category.ModifiedAt = DateTime.Now;
                category.ModifiedBy = currentUser;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Kategori berhasil diupdate!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.TblMAssetCategories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Kategori tidak ditemukan!" });
                }

                // Check if category is being used in assets
                var isUsed = await _context.TblTAssets.AnyAsync(x => x.KategoriBarang == category.KategoriBarang);
                if (isUsed)
                {
                    return BadRequest(new { success = false, message = "Kategori tidak dapat dihapus karena sedang digunakan pada asset!" });
                }

                _context.TblMAssetCategories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Kategori berhasil dihapus!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class CategoryRequest
    {
        public string KategoriBarang { get; set; } = string.Empty;
    }
}