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

    public ProductsController(ProductService service) => _service = service;

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? search = null)
        => Ok(_service.GetAll(search));

    [HttpGet("low-stock")]
    public IActionResult GetLowStock()
        => Ok(_service.GetLowStock());

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var product = _service.GetById(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public IActionResult Create([FromBody] ProductDto dto)
    {
        try
        {
            var product = _service.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] ProductDto dto)
    {
        try
        {
            var product = _service.Update(id, dto);
            return Ok(product);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try { _service.Delete(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
