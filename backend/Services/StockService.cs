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
    private readonly WithdrawDetailRepository _detailRepo;
    private readonly LiteDbContext _ctx;

    public StockService(ProductRepository productRepo, MovementRepository movementRepo,
        WithdrawDetailRepository detailRepo, LiteDbContext ctx)
    {
        _productRepo = productRepo;
        _movementRepo = movementRepo;
        _detailRepo = detailRepo;
        _ctx = ctx;
    }

    public StockMovement AdjustStock(int productId, MovementRequest request, int? companyId)
    {
        var product = _productRepo.GetById(productId);
        if (product == null)
            throw new KeyNotFoundException($"Product {productId} not found");

        // Ownership check
        if (companyId.HasValue && product.CompanyId != companyId.Value)
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
                Note = request.Note,
                CompanyId = product.CompanyId
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

    public void DeleteMovement(int movementId, int? companyId)
    {
        var movement = _movementRepo.GetById(movementId);
        if (movement == null)
            throw new KeyNotFoundException($"Movement {movementId} not found");

        if (companyId.HasValue && movement.CompanyId != companyId.Value)
            throw new KeyNotFoundException($"Movement {movementId} not found");

        var product = _productRepo.GetById(movement.ProductId);
        if (product == null)
            throw new KeyNotFoundException($"Product {movement.ProductId} not found");

        if (companyId.HasValue && product.CompanyId != companyId.Value)
            throw new KeyNotFoundException($"Product {movement.ProductId} not found");

        _ctx.BeginTransaction();
        try
        {
            // Reverse the stock effect: In added qty, Out subtracted qty.
            int delta = movement.Type == MovementType.In ? -movement.Quantity : movement.Quantity;
            int newQty = product.Quantity + delta;

            if (newQty < 0)
                throw new InvalidOperationException(
                    $"Cannot delete: result would be negative stock ({newQty} {product.Unit}).");

            product.Quantity = newQty;
            _productRepo.Update(product);
            _movementRepo.Delete(movementId);

            _ctx.CommitTransaction();
        }
        catch { _ctx.RollbackTransaction(); throw; }
    }

    public IEnumerable<MovementDto> GetMovements(int? productId, int? companyId)
    {
        return _movementRepo.GetAll(productId, companyId).Select(m =>
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

    private static string BuildMovementNote(string? note, string withdrawNo)
        => string.IsNullOrWhiteSpace(note) ? withdrawNo : $"{note} | {withdrawNo}";

    // ===== Withdraws =====

    public WithdrawResult BatchWithdraw(WithdrawRequest request, int? companyId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("No items to withdraw");

        var withdrawNo = GetNextWithdrawNo();
        var details = new List<WithdrawDetail>();

        _ctx.BeginTransaction();
        try
        {
            var withdraw = new Withdraw
            {
                WithdrawNo = withdrawNo,
                Date = request.Date,
                Note = request.Note,
                Status = WithdrawStatus.Withdrawn,
                CompanyId = companyId
            };
            _ctx.Withdraws.Insert(withdraw);

            foreach (var item in request.Items)
            {
                var (product, owned) = GetOwnedProduct(item.ProductId, companyId);
                if (product == null || !owned)
                    throw new KeyNotFoundException($"Product {item.ProductId} not found");

                if (item.Quantity > product.Quantity)
                    throw new InvalidOperationException(
                        $"Cannot withdraw {item.Quantity} of {product.Name}. Only {product.Quantity} in stock.");

                _movementRepo.Insert(new StockMovement
                {
                    ProductId = item.ProductId,
                    Type = MovementType.Out,
                    Quantity = item.Quantity,
                    Note = BuildMovementNote(request.Note, withdrawNo),
                    CompanyId = companyId
                });

                product.Quantity -= item.Quantity;
                _productRepo.Update(product);

                details.Add(new WithdrawDetail
                {
                    WithdrawNo = withdraw.WithdrawNo,
                    ProductId = item.ProductId,
                    Sku = product.Sku,
                    ProductName = product.Name,
                    InStock = product.Quantity,
                    Quantity = item.Quantity,
                    Cost = product.Cost,
                    Price = product.Price,
                    Profit = product.Profit
                });
            }

            _detailRepo.InsertBulk(details);
            _ctx.CommitTransaction();

            return new WithdrawResult { WithdrawNo = withdrawNo, Date = request.Date, ProcessedCount = details.Count };
        }
        catch { _ctx.RollbackTransaction(); throw; }
    }

    public WithdrawResult SaveDraft(WithdrawRequest request, string? existingWithdrawNo, int? companyId)
    {
        if (request.Items == null || request.Items.Count == 0)
            throw new InvalidOperationException("No items to save");

        _ctx.BeginTransaction();
        try
        {
            Withdraw withdraw;

            if (!string.IsNullOrEmpty(existingWithdrawNo))
            {
                withdraw = _ctx.Withdraws.Query().Where(w => w.WithdrawNo == existingWithdrawNo).FirstOrDefault();
                if (withdraw == null)
                    throw new KeyNotFoundException($"Withdraw {existingWithdrawNo} not found");
                if (companyId.HasValue && withdraw.CompanyId != companyId.Value)
                    throw new KeyNotFoundException($"Withdraw {existingWithdrawNo} not found");
                if (withdraw.Status == WithdrawStatus.Withdrawn)
                    throw new InvalidOperationException("Cannot modify a withdrawn record");

                // Update editable master fields on existing draft
                withdraw.Date = request.Date;
                withdraw.Note = request.Note;
                _ctx.Withdraws.Update(withdraw);

                _detailRepo.DeleteByWithdrawNo(withdraw.WithdrawNo);
            }
            else
            {
                withdraw = new Withdraw
                {
                    WithdrawNo = GetNextWithdrawNo(),
                    Date = request.Date,
                    Note = request.Note,
                    Status = WithdrawStatus.Saved,
                    CompanyId = companyId
                };
                _ctx.Withdraws.Insert(withdraw);
            }

            var details = new List<WithdrawDetail>();
            foreach (var item in request.Items)
            {
                var (product, owned) = GetOwnedProduct(item.ProductId, companyId);
                if (product == null || !owned)
                    throw new KeyNotFoundException($"Product {item.ProductId} not found");

                details.Add(new WithdrawDetail
                {
                    WithdrawNo = withdraw.WithdrawNo,
                    ProductId = item.ProductId,
                    Sku = product.Sku,
                    ProductName = product.Name,
                    InStock = product.Quantity,
                    Quantity = item.Quantity,
                    Cost = product.Cost,
                    Price = product.Price,
                    Profit = product.Profit
                });
            }

            _detailRepo.InsertBulk(details);
            _ctx.CommitTransaction();

            return new WithdrawResult { WithdrawNo = withdraw.WithdrawNo, Date = withdraw.Date, ProcessedCount = details.Count };
        }
        catch { _ctx.RollbackTransaction(); throw; }
    }

    public WithdrawResult ConfirmWithdraw(string withdrawNo, int? companyId)
    {
        var withdraw = _ctx.Withdraws.Query().Where(w => w.WithdrawNo == withdrawNo).FirstOrDefault();
        if (withdraw == null)
            throw new KeyNotFoundException($"Withdraw {withdrawNo} not found");
        if (companyId.HasValue && withdraw.CompanyId != companyId.Value)
            throw new KeyNotFoundException($"Withdraw {withdrawNo} not found");
        if (withdraw.Status == WithdrawStatus.Withdrawn)
            throw new InvalidOperationException("Already withdrawn");

        var details = _detailRepo.GetByWithdrawNo(withdrawNo);
        if (details.Count == 0)
            throw new InvalidOperationException("No items to withdraw");

        _ctx.BeginTransaction();
        try
        {
            foreach (var detail in details)
            {
                var product = _productRepo.GetById(detail.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {detail.ProductId} not found");

                if (detail.Quantity > product.Quantity)
                    throw new InvalidOperationException(
                        $"Cannot withdraw {detail.Quantity} of {product.Name}. Only {product.Quantity} in stock.");

                _movementRepo.Insert(new StockMovement
                {
                    ProductId = detail.ProductId,
                    Type = MovementType.Out,
                    Quantity = detail.Quantity,
                    Note = BuildMovementNote(withdraw.Note, withdraw.WithdrawNo),
                    CompanyId = withdraw.CompanyId
                });

                product.Quantity -= detail.Quantity;
                _productRepo.Update(product);
            }

            withdraw.Status = WithdrawStatus.Withdrawn;
            _ctx.Withdraws.Update(withdraw);

            _ctx.CommitTransaction();
            return new WithdrawResult { WithdrawNo = withdraw.WithdrawNo, Date = withdraw.Date, ProcessedCount = details.Count };
        }
        catch { _ctx.RollbackTransaction(); throw; }
    }

    public WithdrawResult CancelWithdraw(string withdrawNo, int? companyId)
    {
        var withdraw = _ctx.Withdraws.Query().Where(w => w.WithdrawNo == withdrawNo).FirstOrDefault();
        if (withdraw == null)
            throw new KeyNotFoundException($"Withdraw {withdrawNo} not found");
        if (companyId.HasValue && withdraw.CompanyId != companyId.Value)
            throw new KeyNotFoundException($"Withdraw {withdrawNo} not found");
        if (withdraw.Status != WithdrawStatus.Withdrawn)
            throw new InvalidOperationException("Only withdrawn records can be cancelled");

        var details = _detailRepo.GetByWithdrawNo(withdrawNo);
        if (details.Count == 0)
            throw new InvalidOperationException("No items to return");

        _ctx.BeginTransaction();
        try
        {
            foreach (var detail in details)
            {
                var product = _productRepo.GetById(detail.ProductId);
                if (product == null)
                    throw new KeyNotFoundException($"Product {detail.ProductId} not found");

                // Return the withdrawn quantity back to stock
                product.Quantity += detail.Quantity;
                _productRepo.Update(product);

                // Log the return as a stock-in movement
                _movementRepo.Insert(new StockMovement
                {
                    ProductId = detail.ProductId,
                    Type = MovementType.In,
                    Quantity = detail.Quantity,
                    Note = $"ยกเลิกเบิก {withdrawNo}",
                    CompanyId = withdraw.CompanyId
                });
            }

            withdraw.Status = WithdrawStatus.Cancelled;
            _ctx.Withdraws.Update(withdraw);

            _ctx.CommitTransaction();
            return new WithdrawResult { WithdrawNo = withdraw.WithdrawNo, Date = withdraw.Date, ProcessedCount = details.Count };
        }
        catch { _ctx.RollbackTransaction(); throw; }
    }

    public List<WithdrawDto> GetWithdraws(int? companyId)
    {
        var query = _ctx.Withdraws.Query();
        if (companyId.HasValue)
            query = query.Where(w => w.CompanyId == companyId.Value);

        return query.ToList()
            .OrderByDescending(w => w.Date)
            .ThenByDescending(w => w.Id)
            .Select(w =>
            {
                var details = _detailRepo.GetByWithdrawNo(w.WithdrawNo);
                return new WithdrawDto
                {
                    Id = w.Id,
                    WithdrawNo = w.WithdrawNo,
                    Date = w.Date,
                    Note = w.Note,
                    Status = w.Status,
                    CompanyId = w.CompanyId,
                    CreatedAt = w.CreatedAt,
                    ItemCount = details.Count,
                    TotalQuantity = details.Sum(d => d.Quantity),
                    TotalPrice = details.Sum(d => d.PriceTotal),
                    TotalProfit = details.Sum(d => d.ProfitTotal)
                };
            })
            .ToList();
    }

    public WithdrawDto? GetWithdraw(string withdrawNo, int? companyId)
    {
        var w = _ctx.Withdraws.Query().Where(x => x.WithdrawNo == withdrawNo).FirstOrDefault();
        if (w == null) return null;
        if (companyId.HasValue && w.CompanyId != companyId.Value) return null;

        var details = _detailRepo.GetByWithdrawNo(w.WithdrawNo);
        return new WithdrawDto
        {
            Id = w.Id,
            WithdrawNo = w.WithdrawNo,
            Date = w.Date,
            Note = w.Note,
            Status = w.Status,
            CompanyId = w.CompanyId,
            CreatedAt = w.CreatedAt,
            ItemCount = details.Count,
            TotalQuantity = details.Sum(d => d.Quantity),
            TotalPrice = details.Sum(d => d.PriceTotal),
            TotalProfit = details.Sum(d => d.ProfitTotal)
        };
    }

    public List<WithdrawDetail> GetWithdrawDetails(string withdrawNo, int? companyId)
    {
        // ownership: verify withdraw belongs to company
        if (companyId.HasValue)
        {
            var w = _ctx.Withdraws.Query().Where(x => x.WithdrawNo == withdrawNo).FirstOrDefault();
            if (w == null || w.CompanyId != companyId.Value) return new List<WithdrawDetail>();
        }
        return _detailRepo.GetByWithdrawNo(withdrawNo);
    }

    private (Product? product, bool owned) GetOwnedProduct(int productId, int? companyId)
    {
        var product = _productRepo.GetById(productId);
        if (product == null) return (null, false);
        if (companyId.HasValue && product.CompanyId != companyId.Value) return (product, false);
        return (product, true);
    }
}
