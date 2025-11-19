using CRM.Application.Common;
using CRM.Application.Common.Caching;
using CRM.Application.Common.Exceptions;
using CRM.Application.Common.Pagination;
using CRM.Domain.Products;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Products;

public class ProductService : IProductService
{
    private readonly IRepository<LumberVariant> _repository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IRepository<LumberVariant> repository,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<LumberVariantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var variant = await _context.LumberVariants.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (variant == null)
        {
            return null;
        }

        return new LumberVariantDto(
            variant.Id,
            variant.Name,
            variant.Species,
            variant.Grade,
            variant.StandardVolume,
            variant.UnitOfMeasure,
            variant.Notes,
            variant.CreatedAt,
            variant.CreatedBy,
            variant.LastModifiedAt,
            variant.LastModifiedBy,
            variant.RowVersion);
    }

    public async Task<IReadOnlyList<LumberVariantListItemDto>> GetAllAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.LumberVariants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Name.Contains(search) ||
                (v.Species != null && v.Species.Contains(search)) ||
                (v.Grade != null && v.Grade.Contains(search)));
        }

        var variants = await query
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);

        return variants.Select(v => new LumberVariantListItemDto(
            v.Id,
            v.Name,
            v.Species,
            v.Grade,
            v.UnitOfMeasure)).ToList();
    }

    public async Task<PagedResult<LumberVariantListItemDto>> GetAllPagedAsync(PaginationRequest pagination, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = _context.LumberVariants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.Name.Contains(search) ||
                (v.Species != null && v.Species.Contains(search)) ||
                (v.Grade != null && v.Grade.Contains(search)));
        }

        var pagedResult = await query
            .OrderBy(v => v.Name)
            .ToPagedResultAsync(pagination, cancellationToken);

        var items = pagedResult.Items.Select(v => new LumberVariantListItemDto(
            v.Id,
            v.Name,
            v.Species,
            v.Grade,
            v.UnitOfMeasure)).ToList();

        return new PagedResult<LumberVariantListItemDto>(items, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
    }

    public async Task<Guid> CreateAsync(CreateLumberVariantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating lumber variant: {Name}, Species: {Species}, Grade: {Grade}",
                request.Name, request.Species, request.Grade);

            var variant = new LumberVariant(
                Guid.NewGuid(),
                request.Name,
                request.Species,
                request.Grade,
                request.StandardVolume,
                request.UnitOfMeasure);

            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                variant.Update(request.Name, request.Species, request.Grade,
                    request.StandardVolume, request.UnitOfMeasure, request.Notes);
            }

            await _repository.AddAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Lumber variant created successfully: {VariantId}, Name: {Name}",
                variant.Id, variant.Name);

            return variant.Id;
        }
        catch (Exception ex) when (ex is not NotFoundException && ex is not BadRequestException && ex is not ValidationException)
        {
            _logger.LogError(ex, "Error creating lumber variant: {Name}", request.Name);
            throw;
        }
    }

    public async Task UpdateAsync(UpdateLumberVariantRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating lumber variant: {VariantId}, Name: {Name}",
                request.Id, request.Name);

            var variant = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (variant == null)
            {
                throw new NotFoundException(nameof(LumberVariant), request.Id);
            }

            // Set RowVersion for optimistic concurrency control
            variant.RowVersion = request.RowVersion;

            variant.Update(request.Name, request.Species, request.Grade,
                request.StandardVolume, request.UnitOfMeasure, request.Notes);

            await _repository.UpdateAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Lumber variant updated successfully: {VariantId}, Name: {Name}",
                variant.Id, variant.Name);
        }
        catch (Exception ex) when (ex is not NotFoundException && ex is not BadRequestException && ex is not ValidationException)
        {
            _logger.LogError(ex, "Error updating lumber variant: {VariantId}", request.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting lumber variant: {VariantId}", id);

            var variant = await _repository.GetByIdAsync(id, cancellationToken);
            if (variant == null)
            {
                throw new NotFoundException(nameof(LumberVariant), id);
            }

            // Check if variant is used in any shipments
            var isUsed = await _context.ShipmentItems
                .AnyAsync(si => si.VariantId == id, cancellationToken);

            if (isUsed)
            {
                throw new BadRequestException("Bu ürün varyantı sevkiyatlarda kullanıldığı için silinemez.");
            }

            await _repository.DeleteAsync(variant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Lumber variant deleted successfully: {VariantId}, Name: {Name}",
                id, variant.Name);
        }
        catch (Exception ex) when (ex is not NotFoundException && ex is not BadRequestException && ex is not ValidationException)
        {
            _logger.LogError(ex, "Error deleting lumber variant: {VariantId}", id);
            throw;
        }
    }
}

