using System.ComponentModel.DataAnnotations;


namespace WebApplication1.Models;


public class Sale
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string RoomNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<SaleItem> Items { get; set; } = new();

}
