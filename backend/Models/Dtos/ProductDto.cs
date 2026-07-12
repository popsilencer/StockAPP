using System.ComponentModel.DataAnnotations;

namespace StockApp.Models.Dtos;

public class ProductDto
{
    [Required] public string Sku { get; set; } = string.Empty;
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required] public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    [Range(0, double.MaxValue)]
    public decimal Cost { get; set; }
    public int ReorderLevel { get; set; }
}
