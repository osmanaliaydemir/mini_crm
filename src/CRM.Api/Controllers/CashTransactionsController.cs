using System.ComponentModel.DataAnnotations;
using CRM.Domain.Finance;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CashTransactionsController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public CashTransactionsController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CashTransactionDto>>> GetTransactions(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] CashTransactionType? type,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.CashTransactions.AsNoTracking();

        if (from.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= to.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new CashTransactionDto(
                t.Id,
                t.TransactionDate,
                t.TransactionType,
                t.Amount,
                t.Currency,
                t.Description,
                t.Category,
                t.RelatedCustomerId,
                t.RelatedShipmentId))
            .ToListAsync(cancellationToken);

        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CashTransactionDto>> GetTransaction(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.CashTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (transaction is null)
        {
            return NotFound();
        }

        var dto = new CashTransactionDto(
            transaction.Id,
            transaction.TransactionDate,
            transaction.TransactionType,
            transaction.Amount,
            transaction.Currency,
            transaction.Description,
            transaction.Category,
            transaction.RelatedCustomerId,
            transaction.RelatedShipmentId);

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CashTransactionDto>> CreateTransaction([FromBody] CashTransactionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var transaction = new CashTransaction(
            Guid.NewGuid(),
            request.TransactionDate,
            request.TransactionType,
            request.Amount,
            request.Currency ?? "TRY",
            request.Description,
            request.Category,
            request.RelatedCustomerId,
            request.RelatedShipmentId);

        _dbContext.CashTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new CashTransactionDto(
            transaction.Id,
            transaction.TransactionDate,
            transaction.TransactionType,
            transaction.Amount,
            transaction.Currency,
            transaction.Description,
            transaction.Category,
            transaction.RelatedCustomerId,
            transaction.RelatedShipmentId);

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CashTransactionDto>> UpdateTransaction(Guid id, [FromBody] CashTransactionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var transaction = await _dbContext.CashTransactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (transaction is null)
        {
            return NotFound();
        }

        transaction.Update(
            request.TransactionDate,
            request.TransactionType,
            request.Amount,
            request.Currency ?? transaction.Currency,
            request.Description,
            request.Category,
            request.RelatedCustomerId,
            request.RelatedShipmentId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new CashTransactionDto(
            transaction.Id,
            transaction.TransactionDate,
            transaction.TransactionType,
            transaction.Amount,
            transaction.Currency,
            transaction.Description,
            transaction.Category,
            transaction.RelatedCustomerId,
            transaction.RelatedShipmentId);

        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.CashTransactions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (transaction is null)
        {
            return NotFound();
        }

        _dbContext.CashTransactions.Remove(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    public sealed record CashTransactionDto(
        Guid Id,
        DateTime TransactionDate,
        CashTransactionType TransactionType,
        decimal Amount,
        string Currency,
        string? Description,
        string? Category,
        Guid? RelatedCustomerId,
        Guid? RelatedShipmentId);

    public sealed class CashTransactionRequest
    {
        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required]
        public CashTransactionType TransactionType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(150)]
        public string? Category { get; set; }

        public Guid? RelatedCustomerId { get; set; }
        public Guid? RelatedShipmentId { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; } = "TRY";
    }
}

