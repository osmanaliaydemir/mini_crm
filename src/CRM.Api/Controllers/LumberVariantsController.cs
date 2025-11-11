using System.ComponentModel.DataAnnotations;
using CRM.Domain.Common.ValueObjects;
using CRM.Domain.Products;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LumberVariantsController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public LumberVariantsController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LumberVariantDto>>> GetVariants(CancellationToken cancellationToken)
    {
        var variants = await _dbContext.LumberVariants
            .AsNoTracking()
            .Select(v => new LumberVariantDto(
                v.Id,
                v.Name,
                v.Species,
                v.Grade,
                v.StandardVolume != null ? new MeasurementDto(v.StandardVolume.Amount, v.StandardVolume.Unit) : null,
                v.UnitOfMeasure,
                v.Notes))
            .ToListAsync(cancellationToken);

        return Ok(variants);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LumberVariantDto>> GetVariant(Guid id, CancellationToken cancellationToken)
    {
        var variant = await _dbContext.LumberVariants
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(v => new LumberVariantDto(
                v.Id,
                v.Name,
                v.Species,
                v.Grade,
                v.StandardVolume != null ? new MeasurementDto(v.StandardVolume.Amount, v.StandardVolume.Unit) : null,
                v.UnitOfMeasure,
                v.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        if (variant is null)
        {
            return NotFound();
        }

        return Ok(variant);
    }

    [HttpPost]
    public async Task<ActionResult<LumberVariantDto>> CreateVariant([FromBody] LumberVariantRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        Measurement? volume = null;
        if (request.StandardVolume is not null)
        {
            volume = Measurement.Create(request.StandardVolume.Amount, request.StandardVolume.Unit);
        }

        var variant = new LumberVariant(
            Guid.NewGuid(),
            request.Name,
            request.Species,
            request.Grade,
            volume,
            request.UnitOfMeasure ?? "m3");

        variant.Update(
            request.Name,
            request.Species,
            request.Grade,
            volume,
            request.UnitOfMeasure ?? "m3",
            request.Notes);

        _dbContext.LumberVariants.Add(variant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new LumberVariantDto(
            variant.Id,
            variant.Name,
            variant.Species,
            variant.Grade,
            variant.StandardVolume != null ? new MeasurementDto(variant.StandardVolume.Amount, variant.StandardVolume.Unit) : null,
            variant.UnitOfMeasure,
            variant.Notes);

        return CreatedAtAction(nameof(GetVariant), new { id = variant.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LumberVariantDto>> UpdateVariant(Guid id, [FromBody] LumberVariantRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var variant = await _dbContext.LumberVariants.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (variant is null)
        {
            return NotFound();
        }

        Measurement? volume = null;
        if (request.StandardVolume is not null)
        {
            volume = Measurement.Create(request.StandardVolume.Amount, request.StandardVolume.Unit);
        }

        variant.Update(
            request.Name,
            request.Species,
            request.Grade,
            volume,
            request.UnitOfMeasure ?? variant.UnitOfMeasure,
            request.Notes);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new LumberVariantDto(
            variant.Id,
            variant.Name,
            variant.Species,
            variant.Grade,
            variant.StandardVolume != null ? new MeasurementDto(variant.StandardVolume.Amount, variant.StandardVolume.Unit) : null,
            variant.UnitOfMeasure,
            variant.Notes);

        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVariant(Guid id, CancellationToken cancellationToken)
    {
        var variant = await _dbContext.LumberVariants.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (variant is null)
        {
            return NotFound();
        }

        _dbContext.LumberVariants.Remove(variant);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    public sealed record LumberVariantDto(
        Guid Id,
        string Name,
        string? Species,
        string? Grade,
        MeasurementDto? StandardVolume,
        string UnitOfMeasure,
        string? Notes);

    public sealed record MeasurementDto(decimal Amount, string Unit);

    public sealed class LumberVariantRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Species { get; set; }

        [MaxLength(50)]
        public string? Grade { get; set; }

        public MeasurementDto? StandardVolume { get; set; }

        [MaxLength(20)]
        public string? UnitOfMeasure { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

