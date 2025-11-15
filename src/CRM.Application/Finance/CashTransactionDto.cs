using CRM.Domain.Finance;

namespace CRM.Application.Finance;

public record CashTransactionDto(
    Guid Id,
    DateTime TransactionDate,
    CashTransactionType TransactionType,
    decimal Amount,
    string Currency,
    string? Description,
    string? Category,
    Guid? RelatedCustomerId,
    string? RelatedCustomerName,
    Guid? RelatedShipmentId,
    string? RelatedShipmentReference,
    DateTime CreatedAt);

public record CreateCashTransactionRequest(
    DateTime TransactionDate,
    CashTransactionType TransactionType,
    decimal Amount,
    string Currency,
    string? Description,
    string? Category,
    Guid? RelatedCustomerId,
    Guid? RelatedShipmentId);

