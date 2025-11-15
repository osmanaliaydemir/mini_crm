namespace CRM.Application.Finance;

public interface IPaymentPlanService
{
    Task<PaymentPlanDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentPlanListItemDto>> GetAllAsync(string? customerSearch = null, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreatePaymentPlanRequest request, CancellationToken cancellationToken = default);
}

