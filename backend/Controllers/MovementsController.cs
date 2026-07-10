using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models.Dtos;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/products/{productId}/movements")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly StockService _stockService;

    public MovementsController(StockService stockService) => _stockService = stockService;

    [HttpPost]
    public IActionResult Create(int productId, [FromBody] MovementRequest request)
    {
        try
        {
            var movement = _stockService.AdjustStock(productId, request);
            return Ok(movement);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
