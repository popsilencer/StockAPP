namespace StockApp.Models.Entities;

public class WithdrawDetail
{
    public int Id { get; set; }
    public string WithdrawNo { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int InStock { get; set; }
    public int Quantity { get; set; }
    // Snapshot of product pricing at time of withdraw. Persisted.
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Profit { get; set; }

    // Computed totals. Not persisted.
    public decimal PriceTotal => Price * Quantity;
    public decimal ProfitTotal => Profit * Quantity;
}
