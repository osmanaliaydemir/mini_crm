using CRM.Domain.Finance;

namespace CRM.Application.Finance;

public record PaymentPlanDto(Guid Id, Guid CustomerId, Guid ShipmentId, PaymentPlanType PlanType, decimal TotalAmount,
    string Currency, DateTime StartDate, int PeriodicityWeeks, string? Notes, string CustomerName, string ShipmentReference, DateTime CreatedAt, byte[] RowVersion);

public record PaymentPlanListItemDto(Guid Id, Guid CustomerId, Guid ShipmentId, PaymentPlanType PlanType, decimal TotalAmount,
    string Currency, DateTime StartDate, string CustomerName, string ShipmentReference);

public record PaymentPlanDetailsDto(PaymentPlanDto Plan, IReadOnlyList<PaymentInstallmentDto> Installments);

public record PaymentInstallmentDto(Guid Id, Guid PaymentPlanId, int InstallmentNumber, DateTime DueDate,
    DateTime? PaidAt, decimal Amount, string Currency, decimal? PaidAmount, string? Notes);

public record CreatePaymentPlanRequest(Guid CustomerId, Guid ShipmentId, PaymentPlanType PlanType, decimal TotalAmount, string? Currency,
    DateTime StartDate, int PeriodicityWeeks, string? Notes, IReadOnlyList<CreatePaymentInstallmentRequest>? Installments);

public record CreatePaymentInstallmentRequest(int InstallmentNumber, DateTime DueDate, decimal Amount);

