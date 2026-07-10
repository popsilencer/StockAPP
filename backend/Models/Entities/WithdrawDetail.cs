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
}
