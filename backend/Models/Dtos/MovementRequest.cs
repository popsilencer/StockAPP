using System.ComponentModel.DataAnnotations;
using StockApp.Models.Entities;

namespace StockApp.Models.Dtos;

public class MovementRequest
{
    [Required] public MovementType Type { get; set; }
    [Range(1, int.MaxValue)] public int Quantity { get; set; }
    public string? Note { get; set; }
}
