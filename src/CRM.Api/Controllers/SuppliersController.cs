using System.ComponentModel.DataAnnotations;
using CRM.Domain.Suppliers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public SuppliersController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers(CancellationToken cancellationToken)
    {
        var suppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .Select(s => new SupplierDto(
                s.Id,
                s.Name,
                s.Country,
                s.TaxNumber,
                s.ContactEmail,
                s.ContactPhone,
                s.AddressLine,
                s.Notes))
            .ToListAsync(cancellationToken);

        return Ok(suppliers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SupplierDto(
                s.Id,
                s.Name,
                s.Country,
                s.TaxNumber,
                s.ContactEmail,
                s.ContactPhone,
                s.AddressLine,
                s.Notes))
            .FirstOrDefaultAsync(cancellationToken);

        if (supplier is null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier([FromBody] SupplierRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var supplier = new Supplier(
            Guid.NewGuid(),
            request.Name,
            request.Country,
            request.TaxNumber,
            request.ContactEmail,
            request.ContactPhone,
            request.AddressLine);

        supplier.Update(
            request.Name,
            request.Country,
            request.TaxNumber,
            request.ContactEmail,
            request.ContactPhone,
            request.AddressLine,
            request.Notes);

        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new SupplierDto(
            supplier.Id,
            supplier.Name,
            supplier.Country,
            supplier.TaxNumber,
            supplier.ContactEmail,
            supplier.ContactPhone,
            supplier.AddressLine,
            supplier.Notes);

        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(Guid id, [FromBody] SupplierRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        supplier.Update(
            request.Name,
            request.Country,
            request.TaxNumber,
            request.ContactEmail,
            request.ContactPhone,
            request.AddressLine,
            request.Notes);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new SupplierDto(
            supplier.Id,
            supplier.Name,
            supplier.Country,
            supplier.TaxNumber,
            supplier.ContactEmail,
            supplier.ContactPhone,
            supplier.AddressLine,
            supplier.Notes);

        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSupplier(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (supplier is null)
        {
            return NotFound();
        }

        _dbContext.Suppliers.Remove(supplier);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    public sealed record SupplierDto(
        Guid Id,
        string Name,
        string? Country,
        string? TaxNumber,
        string? ContactEmail,
        string? ContactPhone,
        string? AddressLine,
        string? Notes);

    public sealed class SupplierRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(300)]
        public string? AddressLine { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

