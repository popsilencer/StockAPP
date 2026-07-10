namespace StockApp.Models.Entities;

public class Company
{
    public int Id { get; set; }
    public string Tax { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
}
