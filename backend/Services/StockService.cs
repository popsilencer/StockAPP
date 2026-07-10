using LiteDB;
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

    public string GetNextWithdrawNo()
    {
        var counter = _ctx.Counters.FindById("withdraw");
        int next = counter == null ? 1 : counter["value"].AsInt32 + 1;
        _ctx.Counters.Upsert(new BsonDocument { ["_id"] = "withdraw", ["value"] = next });
        return $"WD{DateTime.Now:yyyyMMdd}-{next:D4}";
    }

    public WithdrawResult BatchWithdraw(WithdrawRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("No items to withdraw");

        var withdrawNo = GetNextWithdrawNo();
        var lines = new List<WithdrawLine>();

        _ctx.BeginTransaction();
        try
        {
            foreach (var item in request.Items)
            {
                var product = _productRepo.GetById(item.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {item.ProductId} not found");

                if (item.Quantity > product.Quantity)
                    throw new InvalidOperationException(
                        $"Cannot withdraw {item.Quantity} of {product.Name}. Only {product.Quantity} in stock.");

                var movement = new StockMovement
                {
                    ProductId = item.ProductId,
                    Type = MovementType.Out,
                    Quantity = item.Quantity,
                    Note = string.IsNullOrWhiteSpace(request.Note)
                        ? withdrawNo
                        : $"{withdrawNo} | {request.Note}"
                };
                _movementRepo.Insert(movement);

                product.Quantity -= item.Quantity;
                _productRepo.Update(product);

                lines.Add(new WithdrawLine
                {
                    ProductId = item.ProductId,
                    Sku = product.Sku,
                    ProductName = product.Name,
                    Quantity = item.Quantity
                });
            }

            var withdraw = new Withdraw
            {
                WithdrawNo = withdrawNo,
                Date = request.Date,
                Note = request.Note,
                Items = lines
            };
            _ctx.Withdraws.Insert(withdraw);

            _ctx.CommitTransaction();
            return new WithdrawResult
            {
                WithdrawNo = withdrawNo,
                Date = request.Date,
                ProcessedCount = lines.Count
            };
        }
        catch
        {
            _ctx.RollbackTransaction();
            throw;
        }
    }

    public List<Withdraw> GetWithdraws()
        => _ctx.Withdraws.FindAll().OrderByDescending(w => w.Date).ThenByDescending(w => w.Id).ToList();

    public Withdraw? GetWithdraw(int id) => _ctx.Withdraws.FindById(id);
}
