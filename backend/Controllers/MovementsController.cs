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
    private readonly CompanyContext _companyContext;

    public MovementsController(StockService stockService, CompanyContext companyContext)
    {
        _stockService = stockService;
        _companyContext = companyContext;
    }

    private int? Cid => _companyContext.Resolve(HttpContext);

    [HttpGet]
    public IActionResult GetAll([FromQuery] int? productId)
        => Ok(_stockService.GetMovements(productId, Cid));

    [HttpPost("/api/products/{productId:int}/movements")]
    public IActionResult Create(int productId, [FromBody] MovementRequest request)
    {
        try
        {
            var movement = _stockService.AdjustStock(productId, request, Cid);
            return Ok(movement);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
