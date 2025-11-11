using System.ComponentModel.DataAnnotations;
using CRM.Domain.Customers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/customers/{customerId:guid}/interactions")]
[Authorize]
public class CustomerInteractionsController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public CustomerInteractionsController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetInteractions(Guid customerId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Customers.AnyAsync(c => c.Id == customerId, cancellationToken);
        if (!exists)
        {
            return NotFound("Customer not found.");
        }

        var interactions = await _dbContext.CustomerInteractions
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.InteractionDate)
            .Select(i => new CustomerInteractionDto(
                i.Id,
                i.CustomerId,
                i.InteractionDate,
                i.InteractionType,
                i.Subject,
                i.Notes,
                i.RecordedBy))
            .ToListAsync(cancellationToken);

        return Ok(interactions);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerInteractionDto>> CreateInteraction(Guid customerId, [FromBody] CustomerInteractionRequest request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        if (customer is null)
        {
            return NotFound("Customer not found.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var interaction = new CustomerInteraction(
            customerId,
            request.InteractionDate,
            request.InteractionType,
            request.Subject,
            request.Notes,
            request.RecordedBy);

        _dbContext.CustomerInteractions.Add(interaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new CustomerInteractionDto(
            interaction.Id,
            interaction.CustomerId,
            interaction.InteractionDate,
            interaction.InteractionType,
            interaction.Subject,
            interaction.Notes,
            interaction.RecordedBy);

        return CreatedAtAction(nameof(GetInteraction), new { customerId, interactionId = interaction.Id }, dto);
    }

    [HttpGet("{interactionId:guid}")]
    public async Task<ActionResult<CustomerInteractionDto>> GetInteraction(Guid customerId, Guid interactionId, CancellationToken cancellationToken)
    {
        var interaction = await _dbContext.CustomerInteractions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.CustomerId == customerId && i.Id == interactionId, cancellationToken);

        if (interaction is null)
        {
            return NotFound();
        }

        var dto = new CustomerInteractionDto(
            interaction.Id,
            interaction.CustomerId,
            interaction.InteractionDate,
            interaction.InteractionType,
            interaction.Subject,
            interaction.Notes,
            interaction.RecordedBy);

        return Ok(dto);
    }

    [HttpPut("{interactionId:guid}")]
    public async Task<ActionResult<CustomerInteractionDto>> UpdateInteraction(Guid customerId, Guid interactionId, [FromBody] CustomerInteractionRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var interaction = await _dbContext.CustomerInteractions.FirstOrDefaultAsync(i => i.CustomerId == customerId && i.Id == interactionId, cancellationToken);
        if (interaction is null)
        {
            return NotFound();
        }

        interaction.Update(
            request.InteractionDate,
            request.InteractionType,
            request.Subject,
            request.Notes,
            request.RecordedBy);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new CustomerInteractionDto(
            interaction.Id,
            interaction.CustomerId,
            interaction.InteractionDate,
            interaction.InteractionType,
            interaction.Subject,
            interaction.Notes,
            interaction.RecordedBy);

        return Ok(dto);
    }

    [HttpDelete("{interactionId:guid}")]
    public async Task<IActionResult> DeleteInteraction(Guid customerId, Guid interactionId, CancellationToken cancellationToken)
    {
        var interaction = await _dbContext.CustomerInteractions.FirstOrDefaultAsync(i => i.CustomerId == customerId && i.Id == interactionId, cancellationToken);
        if (interaction is null)
        {
            return NotFound();
        }

        _dbContext.CustomerInteractions.Remove(interaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    public sealed record CustomerInteractionDto(
        Guid Id,
        Guid CustomerId,
        DateTime InteractionDate,
        string InteractionType,
        string? Subject,
        string? Notes,
        string? RecordedBy);

    public sealed class CustomerInteractionRequest
    {
        [Required]
        public DateTime InteractionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string InteractionType { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Subject { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(100)]
        public string? RecordedBy { get; set; }
    }
}
