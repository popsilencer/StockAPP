using System.ComponentModel.DataAnnotations;

namespace StockApp.Models.Dtos;

public class WithdrawItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
