using CRM.Application.Common;
using CRM.Domain.Warehouses;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Warehouses;

public class WarehouseService : IWarehouseService
{
    private readonly IRepository<Warehouse> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseService(
        IRepository<Warehouse> repository,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        return warehouse?.Adapt<WarehouseDto>();
    }

    public async Task<IReadOnlyList<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var warehouses = await _context.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        return warehouses.Adapt<List<WarehouseDto>>();
    }

    public async Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        var warehouse = new Warehouse(
            Guid.NewGuid(),
            request.Name,
            request.Location,
            request.ContactPerson,
            request.ContactPhone);

        warehouse.Update(
            request.Name,
            request.Location,
            request.ContactPerson,
            request.ContactPhone,
            request.Notes);

        await _repository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return warehouse.Id;
    }

    public async Task UpdateAsync(UpdateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            throw new InvalidOperationException($"Warehouse with id {request.Id} not found.");
        }

        warehouse.Update(
            request.Name,
            request.Location,
            request.ContactPerson,
            request.ContactPhone,
            request.Notes);

        await _repository.UpdateAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
        {
            throw new InvalidOperationException($"Warehouse with id {id} not found.");
        }

        await _repository.DeleteAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<WarehouseDashboardData> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        const string unspecifiedLocationLabel = "__UNSPECIFIED__";
        
        var warehouses = await _context.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);

        var warehouseDtos = warehouses.Adapt<List<WarehouseDto>>();
        var totalWarehouses = warehouses.Count;

        var locationComparer = StringComparer.OrdinalIgnoreCase;
        var locationStats = warehouses
            .GroupBy(w => NormalizeLocation(w.Location, unspecifiedLocationLabel), locationComparer)
            .Select(group => new WarehouseLocationStat(group.Key, group.Count()))
            .OrderByDescending(stat => stat.WarehouseCount)
            .ThenBy(stat => stat.Location, locationComparer)
            .ToList();

        var distinctLocationCount = locationStats.Count;
        var topLocation = locationStats.FirstOrDefault();
        var topLocationName = topLocation?.Location ?? unspecifiedLocationLabel;
        var topLocationWarehouseCount = topLocation?.WarehouseCount ?? 0;

        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var activeThreshold = now.AddDays(-60);
        var sixMonthsStart = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var periodStart = now.AddDays(-180);

        var recentUnloadingCount = await _context.WarehouseUnloadings
            .AsNoTracking()
            .CountAsync(u => u.UnloadedAt >= thirtyDaysAgo, cancellationToken);

        var activeWarehouseIds = await _context.WarehouseUnloadings
            .AsNoTracking()
            .Where(u => u.UnloadedAt >= activeThreshold)
            .Select(u => u.WarehouseId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeWarehousesCount = activeWarehouseIds.Count;

        var monthlyVolumesRaw = await _context.WarehouseUnloadings
            .AsNoTracking()
            .Where(u => u.UnloadedAt >= sixMonthsStart)
            .GroupBy(u => new { u.UnloadedAt.Year, u.UnloadedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, TotalVolume = g.Sum(x => x.UnloadedVolume) })
            .ToListAsync(cancellationToken);

        var monthlyLookup = monthlyVolumesRaw.ToDictionary(
            keySelector: item => (item.Year, item.Month),
            elementSelector: item => item.TotalVolume);

        var culture = System.Globalization.CultureInfo.CurrentUICulture;
        var labels = new List<string>();
        var values = new List<decimal>();
        var cursor = new DateTime(sixMonthsStart.Year, sixMonthsStart.Month, 1);

        for (var i = 0; i < 6; i++)
        {
            var key = (cursor.Year, cursor.Month);
            monthlyLookup.TryGetValue(key, out var volume);
            labels.Add($"{culture.DateTimeFormat.GetAbbreviatedMonthName(cursor.Month)} {cursor.Year}");
            values.Add(Math.Round(volume, 2));
            cursor = cursor.AddMonths(1);
        }

        var topWarehouseRaw = await _context.WarehouseUnloadings
            .AsNoTracking()
            .Where(u => u.UnloadedAt >= periodStart)
            .GroupBy(u => u.WarehouseId)
            .Select(g => new
            {
                WarehouseId = g.Key,
                TotalVolume = g.Sum(x => x.UnloadedVolume),
                TripCount = g.Count(),
                LastUnloadedAt = g.Max(x => x.UnloadedAt)
            })
            .OrderByDescending(x => x.TotalVolume)
            .ThenByDescending(x => x.LastUnloadedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var warehouseLookup = warehouses.ToDictionary(w => w.Id);

        var topWarehouseStats = topWarehouseRaw
            .Select(item =>
            {
                warehouseLookup.TryGetValue(item.WarehouseId, out var warehouse);
                var name = warehouse?.Name ?? "Warehouse";
                var location = NormalizeLocation(warehouse?.Location, unspecifiedLocationLabel);

                return new TopWarehouseStat(
                    item.WarehouseId,
                    name,
                    location,
                    Math.Round(item.TotalVolume, 2),
                    item.TripCount,
                    item.LastUnloadedAt);
            })
            .ToList();

        return new WarehouseDashboardData(
            warehouseDtos,
            totalWarehouses,
            distinctLocationCount,
            activeWarehousesCount,
            recentUnloadingCount,
            topLocationName,
            topLocationWarehouseCount,
            locationStats,
            topWarehouseStats,
            labels,
            values);
    }

    public async Task<WarehouseDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (warehouse == null)
        {
            return null;
        }

        var warehouseDto = warehouse.Adapt<WarehouseDto>();

        var unloadings = await _context.WarehouseUnloadings
            .AsNoTracking()
            .Where(u => u.WarehouseId == id)
            .OrderByDescending(u => u.UnloadedAt)
            .ToListAsync(cancellationToken);

        var unloadingDtos = unloadings.Select(u => new WarehouseUnloadingDto(
            u.Id,
            u.WarehouseId,
            u.ShipmentId,
            u.TruckPlate,
            u.UnloadedAt,
            u.UnloadedVolume,
            u.Notes,
            u.CreatedAt)).ToList();

        var shipmentOptions = await _context.Shipments
            .AsNoTracking()
            .Select(s => new ShipmentOptionDto(
                s.Id,
                $"{s.ReferenceNumber} - {s.Status}"))
            .ToListAsync(cancellationToken);

        return new WarehouseDetailsDto(warehouseDto, unloadingDtos, shipmentOptions);
    }

    public async Task<Guid> AddUnloadingAsync(AddUnloadingRequest request, CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse == null)
        {
            throw new InvalidOperationException($"Warehouse with id {request.WarehouseId} not found.");
        }

        var unloading = new WarehouseUnloading(
            request.WarehouseId,
            request.ShipmentId,
            request.TruckPlate,
            request.UnloadedAt,
            request.UnloadedVolume);

        unloading.Update(request.UnloadedAt, request.UnloadedVolume, request.Notes);

        _context.WarehouseUnloadings.Add(unloading);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return unloading.Id;
    }

    private static string NormalizeLocation(string? location, string unspecifiedLabel) =>
        string.IsNullOrWhiteSpace(location) ? unspecifiedLabel : location.Trim();
}

