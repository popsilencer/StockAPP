using StockApp.Models.Entities;

namespace StockApp.Models.Dtos;

public class WithdrawDto
{
    public int Id { get; set; }
    public string WithdrawNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public WithdrawStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CompanyId { get; set; }
    public int ItemCount { get; set; }
    public int TotalQuantity { get; set; }
}
