namespace StockApp.Models.Entities;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Cost { get; set; }
    public int ReorderLevel { get; set; }
    public int? CompanyId { get; set; }

    // Computed: total cost of stock on hand (Cost * Quantity). Not persisted.
    public decimal CostTotal => Cost * Quantity;
}
