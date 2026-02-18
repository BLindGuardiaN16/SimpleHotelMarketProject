namespace WebApplication1.Dtos.Sales;

public class RoomChargeCreateDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public List<RoomChargeItemDto> Items { get; set; } = new();
}

public class RoomChargeItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
