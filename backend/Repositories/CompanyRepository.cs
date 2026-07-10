using StockApp.Data;
using StockApp.Models.Entities;

namespace StockApp.Repositories;

public class CompanyRepository
{
    private readonly LiteDbContext _ctx;

    public CompanyRepository(LiteDbContext ctx) => _ctx = ctx;

    public IEnumerable<Company> GetAll() => _ctx.Companies.FindAll();

    public Company? GetById(int id) => _ctx.Companies.FindById(id);

    public Company Insert(Company company)
    {
        _ctx.Companies.Insert(company);
        return company;
    }

    public bool Update(Company company) => _ctx.Companies.Update(company);

    public bool Delete(int id) => _ctx.Companies.Delete(id);

    public bool TaxExists(string tax, int? excludeId = null)
    {
        var companies = _ctx.Companies.Query()
            .Where(c => c.Tax == tax)
            .ToList();

        if (excludeId.HasValue)
            return companies.Any(c => c.Id != excludeId.Value);

        return companies.Any();
    }
}
