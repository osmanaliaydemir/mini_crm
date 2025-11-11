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
public class PaymentPlansController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public PaymentPlansController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentPlanDto>>> GetPlans([FromQuery] Guid? customerId, [FromQuery] Guid? shipmentId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PaymentPlans.AsNoTracking();

        if (customerId.HasValue)
        {
            query = query.Where(plan => plan.CustomerId == customerId.Value);
        }

        if (shipmentId.HasValue)
        {
            query = query.Where(plan => plan.ShipmentId == shipmentId.Value);
        }

        var plans = await query
            .Select(plan => new PaymentPlanDto(
                plan.Id,
                plan.CustomerId,
                plan.ShipmentId,
                plan.PlanType,
                plan.TotalAmount,
                plan.Currency,
                plan.StartDate,
                plan.PeriodicityWeeks,
                plan.Notes))
            .OrderBy(plan => plan.StartDate)
            .ToListAsync(cancellationToken);

        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentPlanDetailsDto>> GetPlan(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _dbContext.PaymentPlans
            .AsNoTracking()
            .Include(plan => plan.Installments)
            .FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);

        if (plan is null)
        {
            return NotFound();
        }

        return Ok(ToDetailsDto(plan));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentPlanDetailsDto>> CreatePlan([FromBody] PaymentPlanRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var plan = new PaymentPlan(
            Guid.NewGuid(),
            request.CustomerId,
            request.ShipmentId,
            request.PlanType,
            request.TotalAmount,
            request.Currency ?? "TRY",
            request.StartDate,
            request.PeriodicityWeeks);

        plan.Update(
            request.PlanType,
            request.TotalAmount,
            request.Currency ?? plan.Currency,
            request.StartDate,
            request.PeriodicityWeeks,
            request.Notes);

        if (request.Installments?.Any() == true)
        {
            var installments = request.Installments.Select(inst =>
                new PaymentInstallment(plan.Id, inst.InstallmentNumber, inst.DueDate, inst.Amount, inst.Currency ?? plan.Currency));

            plan.SetInstallments(installments);
        }

        _dbContext.PaymentPlans.Add(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, ToDetailsDto(plan));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PaymentPlanDetailsDto>> UpdatePlan(Guid id, [FromBody] PaymentPlanRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var plan = await _dbContext.PaymentPlans
            .Include(p => p.Installments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan is null)
        {
            return NotFound();
        }

        plan.Update(
            request.PlanType,
            request.TotalAmount,
            request.Currency ?? plan.Currency,
            request.StartDate,
            request.PeriodicityWeeks,
            request.Notes);

        if (request.Installments?.Any() == true)
        {
            var installments = request.Installments.Select(inst =>
                new PaymentInstallment(plan.Id, inst.InstallmentNumber, inst.DueDate, inst.Amount, inst.Currency ?? plan.Currency));

            plan.SetInstallments(installments);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDetailsDto(plan));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlan(Guid id, CancellationToken cancellationToken = default)
    {
        var plan = await _dbContext.PaymentPlans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (plan is null)
        {
            return NotFound();
        }

        _dbContext.PaymentPlans.Remove(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static PaymentPlanDetailsDto ToDetailsDto(PaymentPlan plan)
    {
        return new PaymentPlanDetailsDto(
            plan.Id,
            plan.CustomerId,
            plan.ShipmentId,
            plan.PlanType,
            plan.TotalAmount,
            plan.Currency,
            plan.StartDate,
            plan.PeriodicityWeeks,
            plan.Notes,
            plan.Installments
                .OrderBy(inst => inst.InstallmentNumber)
                .Select(inst => new PaymentInstallmentDto(
                    inst.Id,
                    inst.PaymentPlanId,
                    inst.InstallmentNumber,
                    inst.DueDate,
                    inst.PaidAt,
                    inst.Amount,
                    inst.Currency,
                    inst.PaidAmount,
                    inst.Notes))
                .ToList());
    }

    public sealed record PaymentPlanDto(
        Guid Id,
        Guid CustomerId,
        Guid ShipmentId,
        PaymentPlanType PlanType,
        decimal TotalAmount,
        string Currency,
        DateTime StartDate,
        int PeriodicityWeeks,
        string? Notes);

    public sealed record PaymentPlanDetailsDto(
        Guid Id,
        Guid CustomerId,
        Guid ShipmentId,
        PaymentPlanType PlanType,
        decimal TotalAmount,
        string Currency,
        DateTime StartDate,
        int PeriodicityWeeks,
        string? Notes,
        IReadOnlyCollection<PaymentInstallmentDto> Installments);

    public sealed record PaymentInstallmentDto(
        Guid Id,
        Guid PaymentPlanId,
        int InstallmentNumber,
        DateTime DueDate,
        DateTime? PaidAt,
        decimal Amount,
        string Currency,
        decimal? PaidAmount,
        string? Notes);

    public sealed class PaymentPlanRequest
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid ShipmentId { get; set; }

        [Required]
        public PaymentPlanType PlanType { get; set; } = PaymentPlanType.Installment;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Range(1, 52)]
        public int PeriodicityWeeks { get; set; } = 4;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<PaymentInstallmentRequest>? Installments { get; set; }
    }

    public sealed class PaymentInstallmentRequest
    {
        [Range(1, int.MaxValue)]
        public int InstallmentNumber { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(10)]
        public string? Currency { get; set; }
    }
}

