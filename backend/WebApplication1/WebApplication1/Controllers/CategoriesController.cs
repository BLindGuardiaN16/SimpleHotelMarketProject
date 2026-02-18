using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos.Categories;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    // ADMIN pasif+aktif
    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAll()
    {
        var items = await _db.Categorys
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(items);
    }

    // SALES sadece aktifler
    [HttpGet("active")]
    public async Task<ActionResult<List<Category>>> GetActive()
    {
        var items = await _db.Categorys
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(items);
    }

    // CREATE
    [HttpPost]
    public async Task<ActionResult<Category>> Create(CategoryCreateDto dto)
    {
        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Name is required.");

        //  (DB collation CI olsa bile burada garantiye alıyoruz)
        var normalized = name.ToUpperInvariant();
        var exists = await _db.Categorys.AnyAsync(c => c.Name.ToUpper() == normalized);
        if (exists)
            return Conflict("Category name must be unique.");

        var entity = new Category
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            IsActive = dto.IsActive ?? true
        };

        _db.Categorys.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // GET by id
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        var entity = await _db.Categorys.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return NotFound();
        return Ok(entity);
    }

    // UPDATE (PUT)
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Category>> Update(int id, CategoryUpdateDto dto)
    {
        var entity = await _db.Categorys.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return NotFound();

        var name = (dto.Name ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Name is required.");

        var normalized = name.ToUpperInvariant();
        var nameTaken = await _db.Categorys.AnyAsync(c => c.Id != id && c.Name.ToUpper() == normalized);
        if (nameTaken)
            return Conflict("Category name must be unique.");

        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    // TOGGLE ACTIVE yani PATCH
    [HttpPatch("{id:int}/toggle-active")]
    public async Task<ActionResult<Category>> ToggleActive(int id)
    {
        var entity = await _db.Categorys.FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return NotFound();

        entity.IsActive = !entity.IsActive;
        await _db.SaveChangesAsync();

        return Ok(entity);
    }
}
