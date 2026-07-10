using StockApp.Data;
using StockApp.Models.Dtos;
using StockApp.Models.Entities;
using StockApp.Repositories;

namespace StockApp.Services;

public class StockService
{
    private readonly ProductRepository _productRepo;
    private readonly MovementRepository _movementRepo;
    private readonly LiteDbContext _ctx;

    public StockService(ProductRepository productRepo, MovementRepository movementRepo, LiteDbContext ctx)
    {
        _productRepo = productRepo;
        _movementRepo = movementRepo;
        _ctx = ctx;
    }

    public StockMovement AdjustStock(int productId, MovementRequest request)
    {
        var product = _productRepo.GetById(productId);
        if (product == null)
            throw new KeyNotFoundException($"Product {productId} not found");

        if (request.Type == MovementType.Out && request.Quantity > product.Quantity)
            throw new InvalidOperationException(
                $"Cannot remove {request.Quantity} {product.Unit}. Only {product.Quantity} in stock.");

        _ctx.BeginTransaction();
        try
        {
            var movement = new StockMovement
            {
                ProductId = productId,
                Type = request.Type,
                Quantity = request.Quantity,
                Note = request.Note
            };
            _movementRepo.Insert(movement);

            product.Quantity += request.Type == MovementType.In ? request.Quantity : -request.Quantity;
            _productRepo.Update(product);

            _ctx.CommitTransaction();
            return movement;
        }
        catch
        {
            _ctx.RollbackTransaction();
            throw;
        }
    }

    public IEnumerable<MovementDto> GetMovements(int? productId = null)
    {
        return _movementRepo.GetAll(productId).Select(m =>
        {
            var product = _productRepo.GetById(m.ProductId);
            return new MovementDto
            {
                Id = m.Id,
                ProductId = m.ProductId,
                Sku = product?.Sku ?? "(deleted)",
                ProductName = product?.Name ?? "(deleted)",
                Type = m.Type,
                Quantity = m.Quantity,
                Note = m.Note,
                CreatedAt = m.CreatedAt
            };
        });
    }
}
