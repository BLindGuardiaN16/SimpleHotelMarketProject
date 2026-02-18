using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos.Products;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    // ADMIN: tüm ürünler (aktif+pasif)
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(products);
    }

    // SALES: sadece aktif ürünler
    [HttpGet("active")]
    public async Task<ActionResult<List<Product>>> GetActive()
    {
        var products = await _db.Products
            .Where(p => p.IsActive)
            .Include(p => p.Category)
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        return Ok(products);
    }

    // CREATE
    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        if (dto.UnityPrice < 0)
            return BadRequest("UnitPrice cannot be negative.");

        if (dto.StockQuantity < 0)
            return BadRequest("StockQuantity cannot be negative.");

        var categoryExists = await _db.Categorys
            .AnyAsync(c => c.Id == dto.CategoryId);

        var normalizedName = dto.Name.Trim().ToUpper();

        var nameTaken = await _db.Products.AnyAsync(p =>
            p.CategoryId == dto.CategoryId &&
            p.Name.ToUpper() == normalizedName);

        if (nameTaken)
            return Conflict("Product name must be unique within the category.");

        if (!categoryExists)
            return BadRequest("Invalid CategoryId.");

        var product = new Product
        {
            Name = dto.Name.Trim(),
            CategoryId = dto.CategoryId,
            UnitPrice = dto.UnityPrice,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive ?? true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // ID ile GET
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // UPDATE İŞLEMİ
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> Update(int id, ProductUpdateDto dto)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        if (dto.UnityPrice < 0)
            return BadRequest("UnitPrice cannot be negative.");

        if (dto.StockQuantity < 0)
            return BadRequest("StockQuantity cannot be negative.");

        var categoryExists = await _db.Categorys
            .AnyAsync(c => c.Id == dto.CategoryId);

        if (!categoryExists)
            return BadRequest("Invalid CategoryId.");

        // 👇 UNIQUE Mİ DEĞİL Mİ KONTROLÜ
        var normalizedName = dto.Name.Trim().ToUpper();

        var nameTaken = await _db.Products.AnyAsync(p =>
            p.Id != id &&
            p.CategoryId == dto.CategoryId &&
            p.Name.ToUpper() == normalizedName);

        if (nameTaken)
            return Conflict("Product name must be unique within the category.");

        // 👇 ALANLARI GÜNCELLEDİĞİM YER
        product.Name = dto.Name.Trim();
        product.CategoryId = dto.CategoryId;
        product.UnitPrice = dto.UnityPrice;
        product.StockQuantity = dto.StockQuantity;
        product.IsActive = dto.IsActive ?? true;

        await _db.SaveChangesAsync();

        return Ok(product);
    }


    // TOGGLE ACTIVE İŞLEMİ
    [HttpPatch("{id:int}/toggle-active")]
    public async Task<ActionResult<Product>> ToggleActive(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync();

        return Ok(product);
    }
}
