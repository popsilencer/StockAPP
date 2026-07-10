namespace StockApp.Models.Dtos;

public class WithdrawResult
{
    public string WithdrawNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int ProcessedCount { get; set; }
}
