using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models.Dtos;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/movements")]
[Authorize]
public class MovementsController : ControllerBase
{
    private readonly StockService _stockService;

    public MovementsController(StockService stockService) => _stockService = stockService;

    [HttpGet]
    public IActionResult GetAll([FromQuery] int? productId)
        => Ok(_stockService.GetMovements(productId));

    // Absolute route — POST /api/products/{productId}/movements
    [HttpPost("/api/products/{productId:int}/movements")]
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
