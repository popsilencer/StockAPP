using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace StockApp.Models.Dtos;

public class CompanyDto
{
    public int Id { get; set; }

    [Required]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "Tax must be exactly 13 digits")]
    public string Tax { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string? Address { get; set; }
}
