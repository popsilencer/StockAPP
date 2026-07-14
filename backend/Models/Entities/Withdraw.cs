namespace StockApp.Models.Entities;

public enum WithdrawStatus { Draft = 0, Saved = 1, Withdrawn = 2, Cancelled = 3 }

public class Withdraw
{
    public int Id { get; set; }
    public string WithdrawNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public WithdrawStatus Status { get; set; } = WithdrawStatus.Draft;
    public int? CompanyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
