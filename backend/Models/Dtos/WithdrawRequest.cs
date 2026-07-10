using System.ComponentModel.DataAnnotations;

namespace StockApp.Models.Dtos;

public class WithdrawRequest
{
    public DateTime Date { get; set; } = DateTime.Today;
    public string? Note { get; set; }
    [Required]
    public List<WithdrawItemDto> Items { get; set; } = new();
}
