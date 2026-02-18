using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

}
