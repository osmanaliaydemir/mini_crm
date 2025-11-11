using System.ComponentModel.DataAnnotations;
using CRM.Infrastructure.Persistence;
using CRM.Domain.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarehousesController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public WarehousesController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses(CancellationToken cancellationToken)
    {
        var warehouses = await _dbContext.Warehouses
            .AsNoTracking()
            .Select(w => new WarehouseDto(
                w.Id,
                w.Name,
                w.Location,
                w.ContactPerson,
                w.ContactPhone,
                w.Notes))
            .ToListAsync(cancellationToken);

        return Ok(warehouses);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse([FromBody] WarehouseRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var warehouse = new Warehouse(Guid.NewGuid(), request.Name, request.Location, request.ContactPerson, request.ContactPhone);
        warehouse.Update(request.Name, request.Location, request.ContactPerson, request.ContactPhone, request.Notes);

        _dbContext.Warehouses.Add(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Location, warehouse.ContactPerson, warehouse.ContactPhone, warehouse.Notes);
        return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseDetailsDto>> GetWarehouse(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses
            .AsNoTracking()
            .Include(w => w.Unloadings)
            .ThenInclude(u => u.Shipment)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (warehouse is null)
        {
            return NotFound();
        }

        var dto = new WarehouseDetailsDto(
            warehouse.Id,
            warehouse.Name,
            warehouse.Location,
            warehouse.ContactPerson,
            warehouse.ContactPhone,
            warehouse.Notes,
            warehouse.Unloadings.Select(u => new WarehouseUnloadingDto(
                u.Id,
                u.ShipmentId,
                u.TruckPlate,
                u.UnloadedAt,
                u.UnloadedVolume,
                u.Notes)).ToList());

        return Ok(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> UpdateWarehouse(Guid id, [FromBody] WarehouseRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        warehouse.Update(request.Name, request.Location, request.ContactPerson, request.ContactPhone, request.Notes);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Location, warehouse.ContactPerson, warehouse.ContactPhone, warehouse.Notes);
        return Ok(dto);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWarehouse(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        _dbContext.Warehouses.Remove(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{warehouseId:guid}/unloadings")]
    public async Task<ActionResult<WarehouseUnloadingDto>> RegisterUnloading(Guid warehouseId, [FromBody] WarehouseUnloadingRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == warehouseId, cancellationToken);
        if (!warehouseExists)
        {
            return NotFound("Warehouse not found.");
        }

        var shipmentExists = await _dbContext.Shipments.AnyAsync(s => s.Id == request.ShipmentId, cancellationToken);
        if (!shipmentExists)
        {
            return BadRequest("Shipment does not exist.");
        }

        var unloading = new WarehouseUnloading(
            warehouseId,
            request.ShipmentId,
            request.TruckPlate,
            request.UnloadedAt,
            request.UnloadedVolume);

        unloading.Update(request.UnloadedAt, request.UnloadedVolume, request.Notes);

        _dbContext.WarehouseUnloadings.Add(unloading);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = new WarehouseUnloadingDto(
            unloading.Id,
            unloading.ShipmentId,
            unloading.TruckPlate,
            unloading.UnloadedAt,
            unloading.UnloadedVolume,
            unloading.Notes);

        return CreatedAtAction(nameof(GetWarehouse), new { id = warehouseId }, dto);
    }

    public sealed record WarehouseDto(Guid Id, string Name, string? Location, string? ContactPerson, string? ContactPhone, string? Notes);

    public sealed record WarehouseDetailsDto(
        Guid Id,
        string Name,
        string? Location,
        string? ContactPerson,
        string? ContactPhone,
        string? Notes,
        IReadOnlyCollection<WarehouseUnloadingDto> Unloadings);

    public sealed record WarehouseUnloadingDto(Guid Id, Guid ShipmentId, string TruckPlate, DateTime UnloadedAt, decimal UnloadedVolume, string? Notes);

    public sealed class WarehouseRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(150)]
        public string? ContactPerson { get; set; }

        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class WarehouseUnloadingRequest
    {
        [Required]
        public Guid ShipmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TruckPlate { get; set; } = string.Empty;

        [Required]
        public DateTime UnloadedAt { get; set; }

        [Range(0.0, double.MaxValue)]
        public decimal UnloadedVolume { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

