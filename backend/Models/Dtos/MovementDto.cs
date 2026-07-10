using StockApp.Models.Entities;

namespace StockApp.Models.Dtos;

public class MovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
