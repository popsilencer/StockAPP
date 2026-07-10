namespace StockApp.Models.Entities;

public class WithdrawLine
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class Withdraw
{
    public int Id { get; set; }
    public string WithdrawNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<WithdrawLine> Items { get; set; } = new();
}
