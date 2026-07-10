using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models.Dtos;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/withdraws")]
[Authorize]
public class WithdrawsController : ControllerBase
{
    private readonly StockService _stockService;

    public WithdrawsController(StockService stockService) => _stockService = stockService;

    [HttpGet("next-no")]
    public IActionResult GetNextNo()
        => Ok(new { withdrawNo = _stockService.GetNextWithdrawNo() });

    [HttpGet]
    public IActionResult GetAll()
        => Ok(_stockService.GetWithdraws());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var w = _stockService.GetWithdraw(id);
        return w == null ? NotFound() : Ok(w);
    }

    [HttpPost]
    public IActionResult Create([FromBody] WithdrawRequest request)
    {
        try
        {
            var result = _stockService.BatchWithdraw(request);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
