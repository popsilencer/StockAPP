using StockApp.Models.Dtos;
using StockApp.Models.Entities;
using StockApp.Repositories;

namespace StockApp.Services;

public class CompanyService
{
    private readonly CompanyRepository _repo;

    public CompanyService(CompanyRepository repo) => _repo = repo;

    public IEnumerable<Company> GetAll() => _repo.GetAll();

    public Company? GetById(int id) => _repo.GetById(id);

    public Company Create(CompanyDto dto)
    {
        if (_repo.TaxExists(dto.Tax))
            throw new InvalidOperationException($"Tax '{dto.Tax}' already exists");

        var company = new Company
        {
            Tax = dto.Tax,
            CompanyName = dto.CompanyName,
            Address = dto.Address
        };
        _repo.Insert(company);
        return company;
    }

    public Company Update(int id, CompanyDto dto)
    {
        var company = _repo.GetById(id);
        if (company == null)
            throw new KeyNotFoundException($"Company {id} not found");

        if (_repo.TaxExists(dto.Tax, id))
            throw new InvalidOperationException($"Tax '{dto.Tax}' already exists");

        company.Tax = dto.Tax;
        company.CompanyName = dto.CompanyName;
        company.Address = dto.Address;
        _repo.Update(company);
        return company;
    }

    public bool Delete(int id)
    {
        var company = _repo.GetById(id);
        if (company == null) return false;
        return _repo.Delete(id);
    }
}
