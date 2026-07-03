using System.Data;
using MarketERP.Data;
using MarketERP.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MarketERP.Services;

public interface IStockMovementService
{
    Task<StockMovement> RecordAsync(
        StockMovementCommand command,
        CancellationToken cancellationToken = default);
}

public sealed record StockMovementCommand(
    int ProductId,
    string MovementType,
    string ReasonType,
    int Quantity,
    DateTime MovementDate,
    decimal? UnitCost = null,
    string? SourceType = null,
    int? SourceId = null,
    int? SourceLineId = null,
    string? SourceNo = null,
    string? Description = null,
    int? CreatedByEmployeeId = null,
    int? ReversalOfMovementId = null,
    bool AllowInactiveProduct = false,
    bool EnforceUniqueSourceLine = true);

public sealed class StockMovementService : IStockMovementService
{
    public const string InboundMovement = "Giris";
    public const string OutboundMovement = "Cikis";

    private readonly AppDbContext _context;

    public StockMovementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovement> RecordAsync(
        StockMovementCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        IDbContextTransaction? ownedTransaction = null;
        if (_context.Database.CurrentTransaction == null)
        {
            ownedTransaction = await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
        }

        try
        {
            string? sourceType = Normalize(command.SourceType, 50);
            if (command.EnforceUniqueSourceLine
                && sourceType != null
                && command.SourceId.HasValue
                && command.SourceLineId.HasValue)
            {
                bool sourceAlreadyRecorded = await _context.StockMovements.AnyAsync(
                    movement => movement.SourceType == sourceType
                        && movement.SourceId == command.SourceId
                        && movement.SourceLineId == command.SourceLineId,
                    cancellationToken);

                if (sourceAlreadyRecorded)
                {
                    throw new InvalidOperationException(
                        "Bu kaynak belge satırı için stok hareketi daha önce oluşturulmuş.");
                }
            }

            var product = await _context.Products
                .FromSqlInterpolated($"SELECT * FROM products WHERE id = {command.ProductId} FOR UPDATE")
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("Ürün bulunamadı.");

            if (!product.IsActive && !command.AllowInactiveProduct)
            {
                throw new InvalidOperationException(
                    "Pasif ürüne stok girişi yapılamaz. Önce ürünü aktifleştirin.");
            }

            int previousQuantity = product.StockQuantity;
            int delta = command.MovementType == InboundMovement
                ? command.Quantity
                : -command.Quantity;
            int newQuantity = checked(previousQuantity + delta);

            if (newQuantity < 0)
            {
                throw new InvalidOperationException("Stok hareketi negatif stok oluşturamaz.");
            }

            product.StockQuantity = newQuantity;

            var movement = new StockMovement
            {
                ProductId = product.Id,
                MovementType = command.MovementType,
                ReasonType = command.ReasonType.Trim(),
                Quantity = command.Quantity,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                UnitCost = command.UnitCost ?? product.PurchasePrice,
                MovementDate = command.MovementDate,
                SourceType = sourceType,
                SourceId = command.SourceId,
                SourceLineId = command.SourceLineId,
                SourceNo = Normalize(command.SourceNo, 100),
                Description = Normalize(command.Description, 1000),
                CreatedByEmployeeId = command.CreatedByEmployeeId,
                CreatedAt = DateTime.Now,
                ReversalOfMovementId = command.ReversalOfMovementId
            };

            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync(cancellationToken);

            if (ownedTransaction != null)
            {
                await ownedTransaction.CommitAsync(cancellationToken);
            }

            return movement;
        }
        catch
        {
            if (ownedTransaction != null)
            {
                await ownedTransaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (ownedTransaction != null)
            {
                await ownedTransaction.DisposeAsync();
            }
        }
    }

    private static void Validate(StockMovementCommand command)
    {
        if (command.ProductId <= 0)
        {
            throw new ArgumentException("Geçerli bir ürün seçilmelidir.", nameof(command));
        }

        if (command.MovementType != InboundMovement
            && command.MovementType != OutboundMovement)
        {
            throw new ArgumentException("Geçersiz stok hareket tipi.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.ReasonType))
        {
            throw new ArgumentException("Stok hareket nedeni zorunludur.", nameof(command));
        }

        if (command.Quantity <= 0)
        {
            throw new ArgumentException("Stok hareket miktarı sıfırdan büyük olmalıdır.", nameof(command));
        }
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }
}
