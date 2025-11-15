using System.Globalization;

namespace CRM.Application.Finance;

public record CashTransactionDashboardData(
    IReadOnlyList<CashTransactionDto> Transactions,
    CashSummary Summary,
    IReadOnlyList<CategorySummary> CategorySummaries,
    IReadOnlyList<string> MonthlyLabels,
    IReadOnlyList<decimal> MonthlyIncomeData,
    IReadOnlyList<decimal> MonthlyExpenseData,
    IReadOnlyList<decimal> MonthlyNetData);

public record CashSummary(
    decimal TotalIncome,
    decimal TotalExpense)
{
    public decimal NetBalance => TotalIncome - TotalExpense;
}

public record CategorySummary(
    string? CategoryName,
    decimal IncomeAmount,
    decimal ExpenseAmount)
{
    public decimal NetAmount => IncomeAmount - ExpenseAmount;
    public decimal TotalMovement => IncomeAmount + ExpenseAmount;
}

