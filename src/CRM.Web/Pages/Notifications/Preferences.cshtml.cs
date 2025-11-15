using System.ComponentModel.DataAnnotations;
using CRM.Domain.Notifications;
using CRM.Infrastructure.Identity;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CRM.Web.Pages.Notifications;

[Authorize]
public class PreferencesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CRMDbContext _dbContext;
    private readonly ILogger<PreferencesModel> _logger;

    public PreferencesModel(
        UserManager<ApplicationUser> userManager,
        CRMDbContext dbContext,
        ILogger<PreferencesModel> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    [BindProperty]
    public NotificationPreferencesInput Preferences { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        var notificationPreferences = await _dbContext.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(np => np.UserId == user.Id, cancellationToken);

        if (notificationPreferences is not null)
        {
            Preferences.EmailShipmentUpdates = notificationPreferences.EmailShipmentUpdates;
            Preferences.EmailPaymentReminders = notificationPreferences.EmailPaymentReminders;
            Preferences.EmailWarehouseAlerts = notificationPreferences.EmailWarehouseAlerts;
            Preferences.EmailCustomerInteractions = notificationPreferences.EmailCustomerInteractions;
            Preferences.EmailSystemAnnouncements = notificationPreferences.EmailSystemAnnouncements;
            Preferences.InAppShipmentUpdates = notificationPreferences.InAppShipmentUpdates;
            Preferences.InAppPaymentReminders = notificationPreferences.InAppPaymentReminders;
            Preferences.InAppWarehouseAlerts = notificationPreferences.InAppWarehouseAlerts;
            Preferences.InAppCustomerInteractions = notificationPreferences.InAppCustomerInteractions;
            Preferences.InAppSystemAnnouncements = notificationPreferences.InAppSystemAnnouncements;
        }
        else
        {
            // Varsayılan değerler
            Preferences.EmailShipmentUpdates = true;
            Preferences.EmailPaymentReminders = true;
            Preferences.EmailWarehouseAlerts = true;
            Preferences.EmailCustomerInteractions = false;
            Preferences.EmailSystemAnnouncements = true;
            Preferences.InAppShipmentUpdates = true;
            Preferences.InAppPaymentReminders = true;
            Preferences.InAppWarehouseAlerts = true;
            Preferences.InAppCustomerInteractions = true;
            Preferences.InAppSystemAnnouncements = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        try
        {
            var notificationPreferences = await _dbContext.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == user.Id, cancellationToken);

            if (notificationPreferences is null)
            {
                // Yeni kayıt oluştur
                notificationPreferences = new NotificationPreferences(
                    Guid.NewGuid(),
                    user.Id,
                    Preferences.EmailShipmentUpdates,
                    Preferences.EmailPaymentReminders,
                    Preferences.EmailWarehouseAlerts,
                    Preferences.EmailCustomerInteractions,
                    Preferences.EmailSystemAnnouncements,
                    Preferences.InAppShipmentUpdates,
                    Preferences.InAppPaymentReminders,
                    Preferences.InAppWarehouseAlerts,
                    Preferences.InAppCustomerInteractions,
                    Preferences.InAppSystemAnnouncements);

                await _dbContext.NotificationPreferences.AddAsync(notificationPreferences, cancellationToken);
            }
            else
            {
                // Mevcut kaydı güncelle
                notificationPreferences.Update(
                    Preferences.EmailShipmentUpdates,
                    Preferences.EmailPaymentReminders,
                    Preferences.EmailWarehouseAlerts,
                    Preferences.EmailCustomerInteractions,
                    Preferences.EmailSystemAnnouncements,
                    Preferences.InAppShipmentUpdates,
                    Preferences.InAppPaymentReminders,
                    Preferences.InAppWarehouseAlerts,
                    Preferences.InAppCustomerInteractions,
                    Preferences.InAppSystemAnnouncements);

                _dbContext.NotificationPreferences.Update(notificationPreferences);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Notification preferences updated. UserId: {UserId}, EmailShipmentUpdates: {EmailShipmentUpdates}",
                user.Id, Preferences.EmailShipmentUpdates);

            TempData["StatusMessage"] = "Notification preferences updated successfully.";
            TempData["StatusMessageType"] = "success";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences. UserId: {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating your preferences. Please try again.");
            return Page();
        }
    }

    public sealed class NotificationPreferencesInput
    {
        [Display(Name = "Email Shipment Updates")]
        public bool EmailShipmentUpdates { get; set; }

        [Display(Name = "Email Payment Reminders")]
        public bool EmailPaymentReminders { get; set; }

        [Display(Name = "Email Warehouse Alerts")]
        public bool EmailWarehouseAlerts { get; set; }

        [Display(Name = "Email Customer Interactions")]
        public bool EmailCustomerInteractions { get; set; }

        [Display(Name = "Email System Announcements")]
        public bool EmailSystemAnnouncements { get; set; }

        [Display(Name = "In-App Shipment Updates")]
        public bool InAppShipmentUpdates { get; set; }

        [Display(Name = "In-App Payment Reminders")]
        public bool InAppPaymentReminders { get; set; }

        [Display(Name = "In-App Warehouse Alerts")]
        public bool InAppWarehouseAlerts { get; set; }

        [Display(Name = "In-App Customer Interactions")]
        public bool InAppCustomerInteractions { get; set; }

        [Display(Name = "In-App System Announcements")]
        public bool InAppSystemAnnouncements { get; set; }
    }
}

