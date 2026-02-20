using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dtos.Sales;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SalesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("room-charge")]
    public async Task<ActionResult<object>> CreateRoomCharge(RoomChargeCreateDto dto)
    {
        var room = (dto.RoomNumber ?? "").Trim();
        if (string.IsNullOrWhiteSpace(room))
            return BadRequest("RoomNumber is required.");

        if (dto.Items == null || dto.Items.Count == 0)
            return BadRequest("Items is required.");

        if (dto.Items.Any(i => i.Quantity <= 0))
            return BadRequest("Quantity must be > 0.");

        // aynı product birden fazla geldiyse grupla
        var grouped = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        var productIds = grouped.Select(x => x.ProductId).ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest("One or more ProductId values are invalid.");

        decimal total = 0m;

        foreach (var line in grouped)
        {
            var p = products.First(x => x.Id == line.ProductId);

            if (!p.IsActive)
                return BadRequest($"Product '{p.Name}' is inactive.");

            if (line.Quantity > p.StockQuantity)
                return BadRequest($"Not enough stock for '{p.Name}'. Requested={line.Quantity}, Stock={p.StockQuantity}");

            total += p.UnitPrice * line.Quantity;
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var sale = new Sale
        {
            RoomNumber = room,
            TotalAmount = total,
            CreatedAt = DateTime.UtcNow
        };

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync(); // sale.Id oluşsun

        foreach (var line in grouped)
        {
            var p = products.First(x => x.Id == line.ProductId);

            _db.SaleItems.Add(new SaleItem
            {
                SaleId = sale.Id,
                ProductId = p.Id,
                UnitPrice = p.UnitPrice,
                Quantity = line.Quantity
            });

            p.StockQuantity -= line.Quantity;
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(new { saleId = sale.Id, totalAmount = sale.TotalAmount });

    }

    [HttpGet]
    public async Task<ActionResult<List<SaleListItemDto>>> GetAll()
    {
        var list = await _db.Sales
              .AsNoTracking()
              .OrderByDescending(s => s.CreatedAt)
              .Select(s => new SaleListItemDto
              {
                  Id = s.Id,
                  RoomNumber = s.RoomNumber,
                  TotalAmount = s.TotalAmount,
                  CreatedAt = s.CreatedAt
              }).ToListAsync();
       
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDetailDto>> GetById(int id)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SaleDetailDto
            {
                Id = s.Id,
                RoomNumber = s.RoomNumber,
                TotalAmount = s.TotalAmount,
                CreatedAt = s.CreatedAt,
                Items = s.Items.Select(i => new SaleDetailItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product != null ? i.Product.Name : "",
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = i.UnitPrice * i.Quantity
                }).ToList()
            }).FirstOrDefaultAsync();

        if (sale == null) return NotFound();

        return Ok(sale);
    }

}
