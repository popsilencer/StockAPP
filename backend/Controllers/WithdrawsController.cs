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
    private readonly CompanyContext _companyContext;

    public WithdrawsController(StockService stockService, CompanyContext companyContext)
    {
        _stockService = stockService;
        _companyContext = companyContext;
    }

    private int? Cid => _companyContext.Resolve(HttpContext);

    [HttpGet("next-no")]
    public IActionResult GetNextNo()
        => Ok(new { withdrawNo = _stockService.GetNextWithdrawNo() });

    [HttpGet]
    public IActionResult GetAll() => Ok(_stockService.GetWithdraws(Cid));

    [HttpGet("{withdrawNo}")]
    public IActionResult GetByWithdrawNo(string withdrawNo)
    {
        var w = _stockService.GetWithdraw(withdrawNo, Cid);
        return w == null ? NotFound() : Ok(w);
    }

    [HttpGet("{withdrawNo}/details")]
    public IActionResult GetDetails(string withdrawNo)
    {
        var w = _stockService.GetWithdraw(withdrawNo, Cid);
        if (w == null) return NotFound();
        var details = _stockService.GetWithdrawDetails(withdrawNo, Cid);
        return Ok(details);
    }

    [HttpPost("save")]
    public IActionResult Save([FromBody] WithdrawRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = "Validation failed", errors });
        }
        try { return Ok(_stockService.SaveDraft(request, null, Cid)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace }); }
    }

    [HttpPut("{withdrawNo}/save")]
    public IActionResult UpdateDraft(string withdrawNo, [FromBody] WithdrawRequest request)
    {
        try { return Ok(_stockService.SaveDraft(request, withdrawNo, Cid)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{withdrawNo}/confirm")]
    public IActionResult Confirm(string withdrawNo)
    {
        try { return Ok(_stockService.ConfirmWithdraw(withdrawNo, Cid)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost]
    public IActionResult Create([FromBody] WithdrawRequest request)
    {
        try { return Ok(_stockService.BatchWithdraw(request, Cid)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{withdrawNo}/cancel")]
    public IActionResult Cancel(string withdrawNo)
    {
        try { return Ok(_stockService.CancelWithdraw(withdrawNo, Cid)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
