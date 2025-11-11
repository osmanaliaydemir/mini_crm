using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Domain.Enums;
using CRM.Domain.Shipments;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public ShipmentsController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShipmentDto>>> GetShipments(CancellationToken cancellationToken)
    {
        var shipments = await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Supplier)
            .Include(s => s.Customer)
            .Include(s => s.CustomsProcess)
            .Include(s => s.Items)
            .Include(s => s.TransportUnits)
            .Include(s => s.Stages)
            .Select(s => ToDto(s))
            .ToListAsync(cancellationToken);

        return Ok(shipments);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShipmentDto>> GetShipment(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .Include(s => s.Supplier)
            .Include(s => s.Customer)
            .Include(s => s.CustomsProcess)
            .Include(s => s.Items)
            .Include(s => s.TransportUnits)
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment is null)
        {
            return NotFound();
        }

        return Ok(ToDto(shipment));
    }

    [HttpPost]
    public async Task<ActionResult<ShipmentDto>> CreateShipment([FromBody] ShipmentRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var shipment = new Shipment(
            Guid.NewGuid(),
            request.SupplierId,
            request.ReferenceNumber,
            request.ShipmentDate,
            request.Status,
            request.CustomerId);

        shipment.Update(
            request.ShipmentDate,
            request.EstimatedArrival,
            request.Status,
            request.LoadingPort,
            request.DischargePort,
            request.Notes,
            request.CustomerId);

        if (request.Stages?.Any() == true)
        {
            foreach (var stage in request.Stages)
            {
                shipment.SetOrUpdateStage(stage.Status, stage.StartedAt, stage.CompletedAt, stage.Notes);
            }
        }
        else
        {
            shipment.SetOrUpdateStage(request.Status, request.ShipmentDate, null, null);
        }

        if (request.Items?.Any() == true)
        {
            foreach (var item in request.Items)
            {
                shipment.AddItem(item.VariantId, item.Quantity, item.Volume);
            }
        }

        if (request.TransportUnits?.Any() == true)
        {
            foreach (var unit in request.TransportUnits)
            {
                shipment.AddTransportUnit(unit.Mode, unit.Identifier ?? string.Empty, unit.Count);
            }
        }

        if (request.Customs is not null)
        {
            var customs = new CustomsProcess(shipment.Id, request.Customs.Status, request.Customs.StartedAt, request.Customs.DocumentNumber);
            customs.Update(request.Customs.Status, request.Customs.CompletedAt, request.Customs.DocumentNumber, request.Customs.Notes);
            shipment.SetCustomsProcess(customs);
        }

        _dbContext.Shipments.Add(shipment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetShipment), new { id = shipment.Id }, ToDto(shipment));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShipmentDto>> UpdateShipment(Guid id, [FromBody] ShipmentRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var shipment = await _dbContext.Shipments
            .Include(s => s.Supplier)
            .Include(s => s.Customer)
            .Include(s => s.CustomsProcess)
            .Include(s => s.Items)
            .Include(s => s.TransportUnits)
            .Include(s => s.Stages)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (shipment is null)
        {
            return NotFound();
        }

        shipment.ReassignSupplier(request.SupplierId);
        shipment.ReassignCustomer(request.CustomerId);
        shipment.Update(
            request.ShipmentDate,
            request.EstimatedArrival,
            request.Status,
            request.LoadingPort,
            request.DischargePort,
            request.Notes,
            request.CustomerId);

        if (request.Stages?.Any() == true)
        {
            foreach (var stage in request.Stages)
            {
                shipment.SetOrUpdateStage(stage.Status, stage.StartedAt, stage.CompletedAt, stage.Notes);
            }
        }
        else
        {
            shipment.SetOrUpdateStage(request.Status, DateTime.UtcNow, null, null);
        }

        shipment.ClearItems();
        if (request.Items?.Any() == true)
        {
            foreach (var item in request.Items)
            {
                shipment.AddItem(item.VariantId, item.Quantity, item.Volume);
            }
        }

        shipment.ClearTransportUnits();
        if (request.TransportUnits?.Any() == true)
        {
            foreach (var unit in request.TransportUnits)
            {
                shipment.AddTransportUnit(unit.Mode, unit.Identifier ?? string.Empty, unit.Count);
            }
        }

        if (request.Customs is not null)
        {
            if (shipment.CustomsProcess is null)
            {
                var customs = new CustomsProcess(shipment.Id, request.Customs.Status, request.Customs.StartedAt, request.Customs.DocumentNumber);
                customs.Update(request.Customs.Status, request.Customs.CompletedAt, request.Customs.DocumentNumber, request.Customs.Notes);
                shipment.SetCustomsProcess(customs);
            }
            else
            {
                shipment.CustomsProcess.Update(request.Customs.Status, request.Customs.CompletedAt, request.Customs.DocumentNumber, request.Customs.Notes);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToDto(shipment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteShipment(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (shipment is null)
        {
            return NotFound();
        }

        _dbContext.Shipments.Remove(shipment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ShipmentDto ToDto(Shipment shipment)
    {
        return new ShipmentDto(
            shipment.Id,
            shipment.SupplierId,
            shipment.CustomerId,
            shipment.ReferenceNumber,
            shipment.ShipmentDate,
            shipment.EstimatedArrival,
            shipment.Status,
            shipment.LoadingPort,
            shipment.DischargePort,
            shipment.Notes,
            shipment.Items.Select(i => new ShipmentItemDto(i.VariantId, i.Quantity, i.Volume)).ToList(),
            shipment.TransportUnits.Select(t => new ShipmentTransportDto(t.Mode, t.Identifier, t.Count)).ToList(),
            shipment.Stages
                .OrderBy(s => s.StartedAt)
                .Select(s => new ShipmentStageDto(s.Status, s.StartedAt, s.CompletedAt, s.Notes))
                .ToList(),
            shipment.CustomsProcess is null
                ? null
                : new CustomsProcessDto(
                    shipment.CustomsProcess.Status,
                    shipment.CustomsProcess.StartedAt,
                    shipment.CustomsProcess.CompletedAt,
                    shipment.CustomsProcess.DocumentNumber,
                    shipment.CustomsProcess.Notes));
    }

    public sealed record ShipmentDto(
        Guid Id,
        Guid SupplierId,
        Guid? CustomerId,
        string ReferenceNumber,
        DateTime ShipmentDate,
        DateTime? EstimatedArrival,
        ShipmentStatus Status,
        string? LoadingPort,
        string? DischargePort,
        string? Notes,
        IReadOnlyCollection<ShipmentItemDto> Items,
        IReadOnlyCollection<ShipmentTransportDto> TransportUnits,
        IReadOnlyCollection<ShipmentStageDto> Stages,
        CustomsProcessDto? Customs);

    public sealed record ShipmentItemDto(Guid VariantId, decimal Quantity, decimal Volume);

    public sealed record ShipmentTransportDto(TransportMode Mode, string Identifier, int Count);

    public sealed record ShipmentStageDto(ShipmentStatus Status, DateTime StartedAt, DateTime? CompletedAt, string? Notes);

    public sealed record CustomsProcessDto(CustomsStatus Status, DateTime StartedAt, DateTime? CompletedAt, string? DocumentNumber, string? Notes);

    public sealed class ShipmentRequest
    {
        [Required]
        public Guid SupplierId { get; set; }

        public Guid? CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        public DateTime ShipmentDate { get; set; }

        public DateTime? EstimatedArrival { get; set; }

        [Required]
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Ordered;

        [MaxLength(150)]
        public string? LoadingPort { get; set; }

        [MaxLength(150)]
        public string? DischargePort { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<ShipmentItemRequest>? Items { get; set; }

        public List<ShipmentTransportRequest>? TransportUnits { get; set; }

        public List<ShipmentStageRequest>? Stages { get; set; }

        public CustomsProcessRequest? Customs { get; set; }
    }

    public sealed class ShipmentItemRequest
    {
        [Required]
        public Guid VariantId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0.0, double.MaxValue)]
        public decimal Volume { get; set; }
    }

    public sealed class ShipmentTransportRequest
    {
        [Required]
        public TransportMode Mode { get; set; }

        [MaxLength(100)]
        public string? Identifier { get; set; }

        [Range(1, int.MaxValue)]
        public int Count { get; set; }
    }

    public sealed class ShipmentStageRequest
    {
        [Required]
        public ShipmentStatus Status { get; set; }

        [Required]
        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class CustomsProcessRequest
    {
        [Required]
        public CustomsStatus Status { get; set; } = CustomsStatus.Pending;

        [Required]
        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

