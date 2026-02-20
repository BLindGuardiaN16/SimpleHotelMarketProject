namespace WebApplication1.Dtos.Sales;

public class SaleDetailDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SaleDetailItemDto> Items { get; set; } = new();
}

public class SaleDetailItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
