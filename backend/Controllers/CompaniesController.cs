using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockApp.Models.Dtos;
using StockApp.Services;

namespace StockApp.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly CompanyService _companyService;

    public CompaniesController(CompanyService companyService) => _companyService = companyService;

    [HttpGet]
    public IActionResult GetAll() => Ok(_companyService.GetAll());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var c = _companyService.GetById(id);
        return c == null ? NotFound() : Ok(c);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = string.Join(", ", errors) });
        }
        try
        {
            var company = _companyService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = string.Join(", ", errors) });
        }
        try
        {
            var company = _companyService.Update(id, dto);
            return Ok(company);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var ok = _companyService.Delete(id);
        return ok ? NoContent() : NotFound();
    }
}
