using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models.Dtos;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ProductService _service;
    private readonly CompanyContext _companyContext;

    public ProductsController(ProductService service, CompanyContext companyContext)
    {
        _service = service;
        _companyContext = companyContext;
    }

    private int? Cid => _companyContext.Resolve(HttpContext);

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? search = null)
        => Ok(_service.GetAll(search, Cid));

    [HttpGet("low-stock")]
    public IActionResult GetLowStock()
        => Ok(_service.GetLowStock(Cid));

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _service.GetById(id);
        if (product == null) return NotFound();
        if (Cid.HasValue && product.CompanyId != Cid.Value) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public IActionResult Create([FromBody] ProductDto dto)
    {
        try
        {
            var product = _service.Create(dto, Cid);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { message = ex.Message, detail = ex.InnerException?.Message }); }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] ProductDto dto)
    {
        try
        {
            var product = _service.Update(id, dto, Cid);
            return Ok(product);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try { _service.Delete(id, Cid); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
