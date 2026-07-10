using System.ComponentModel.DataAnnotations;

namespace StockApp.Models.Dtos;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(4)]
    public string NewPassword { get; set; } = string.Empty;
}
