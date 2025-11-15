using CRM.Domain.Finance;

namespace CRM.Application.Finance;

public interface ICashTransactionService
{
    Task<Guid> CreateAsync(CreateCashTransactionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CashTransactionDto>> GetAllAsync(DateTime? from = null, DateTime? to = null,
        CashTransactionType? type = null, CancellationToken cancellationToken = default);
    Task<CashTransactionDashboardData> GetDashboardDataAsync(DateTime? from = null, DateTime? to = null,
        CashTransactionType? type = null, CancellationToken cancellationToken = default);
}

