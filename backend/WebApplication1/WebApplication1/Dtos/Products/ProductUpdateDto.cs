namespace WebApplication1.Dtos.Products
{
    public class ProductUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal UnityPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool? IsActive { get; set; }
    }
}
