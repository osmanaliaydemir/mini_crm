using CRM.Application.Common.Caching;
using CRM.Application.Finance;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Web.ViewComponents;

public class CashboxQuickSummaryViewComponent : ViewComponent
{
    private readonly ICashTransactionService _cashTransactionService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CashboxQuickSummaryViewComponent> _logger;

    public CashboxQuickSummaryViewComponent(
        ICashTransactionService cashTransactionService,
        ICacheService cacheService,
        ILogger<CashboxQuickSummaryViewComponent> logger)
    {
        _cashTransactionService = cashTransactionService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            // Cashbox quick summary için cache (3 dakika)
            var dashboardData = await _cacheService.GetOrCreateAsync(
                CacheKeys.CashboxQuickSummary,
                async () => await _cashTransactionService.GetDashboardDataAsync(),
                TimeSpan.FromMinutes(3),
                default);

            var lastTransaction = dashboardData.RecentTransactions.FirstOrDefault();
            var lastTransactionDate = lastTransaction?.TransactionDate;

            var model = new CashboxQuickSummaryViewModel
            {
                NetBalance = dashboardData.Summary.NetBalance,
                LastTransactionDate = lastTransactionDate
            };

            return View(model);
        }
        catch
        {
            // Hata durumunda boş model döndür
            return View(new CashboxQuickSummaryViewModel
            {
                NetBalance = 0m,
                LastTransactionDate = null
            });
        }
    }

    public class CashboxQuickSummaryViewModel
    {
        public decimal NetBalance { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }
}

