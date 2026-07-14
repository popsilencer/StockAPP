using StockApp.Models.Dtos;
using StockApp.Models.Entities;
using StockApp.Repositories;

namespace StockApp.Services;

public class ProductService
{
    private readonly ProductRepository _repo;
    private readonly MovementRepository _movementRepo;

    public ProductService(ProductRepository repo, MovementRepository movementRepo)
    {
        _repo = repo;
        _movementRepo = movementRepo;
    }

    public IEnumerable<Product> GetAll(string? search, int? companyId)
    {
        if (string.IsNullOrWhiteSpace(search))
            return _repo.GetAll(companyId);
        return _repo.Search(search, companyId);
    }

    public Product? GetById(int id) => _repo.GetById(id);

    public Product Create(ProductDto dto, int? companyId)
    {
        if (companyId.HasValue && _repo.SkuExists(dto.Sku, companyId))
            throw new InvalidOperationException($"SKU '{dto.Sku}' already exists in this company");

        var product = new Product
        {
            Sku = dto.Sku,
            Name = dto.Name,
            Description = dto.Description,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            Cost = dto.Cost,
            Price = dto.Price,
            ReorderLevel = dto.ReorderLevel,
            CompanyId = companyId
        };
        _repo.Insert(product);

        // Create initial stock-in movement
        if (dto.Quantity > 0)
        {
            _movementRepo.Insert(new StockMovement
            {
                ProductId = product.Id,
                Type = MovementType.In,
                Quantity = dto.Quantity,
                Note = "สร้างใหม่",
                CompanyId = companyId
            });
        }

        return product;
    }

    public Product Update(int id, ProductDto dto, int? companyId)
    {
        var existing = _repo.GetById(id);
        if (existing == null)
            throw new KeyNotFoundException($"Product {id} not found");

        // Ownership check (non-admin cannot touch other companies)
        if (companyId.HasValue && existing.CompanyId != companyId.Value)
            throw new KeyNotFoundException($"Product {id} not found");

        if (companyId.HasValue && _repo.SkuExists(dto.Sku, companyId, excludeId: id))
            throw new InvalidOperationException($"SKU '{dto.Sku}' already exists in this company");

        var oldQty = existing.Quantity;

        existing.Sku = dto.Sku;
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Unit = dto.Unit;
        existing.Quantity = dto.Quantity;
        existing.Cost = dto.Cost;
        existing.Price = dto.Price;
        existing.ReorderLevel = dto.ReorderLevel;
        _repo.Update(existing);

        // Log stock adjustment movement when quantity changes via edit
        var delta = dto.Quantity - oldQty;
        if (delta != 0)
        {
            _movementRepo.Insert(new StockMovement
            {
                ProductId = existing.Id,
                Type = delta > 0 ? MovementType.In : MovementType.Out,
                Quantity = Math.Abs(delta),
                Note = "ปรับปรุงสต๊อก",
                CompanyId = existing.CompanyId
            });
        }

        return existing;
    }

    public void Delete(int id, int? companyId)
    {
        var product = _repo.GetById(id);
        if (product == null)
            throw new KeyNotFoundException($"Product {id} not found");

        if (companyId.HasValue && product.CompanyId != companyId.Value)
            throw new KeyNotFoundException($"Product {id} not found");

        _repo.Delete(id);
    }

    public IEnumerable<Product> GetLowStock(int? companyId) => _repo.GetLowStock(companyId);
}
