using CRM.Application.Notifications;
using CRM.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CRM.Web.Pages.Notifications;

[Authorize]
public class PreferencesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PreferencesModel> _logger;

    public PreferencesModel(UserManager<ApplicationUser> userManager, INotificationService notificationService, ILogger<PreferencesModel> logger)
    {
        _userManager = userManager;
        _notificationService = notificationService;
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

        var preferences = await _notificationService.GetPreferencesAsync(user.Id, cancellationToken);

        if (preferences is not null)
        {
            Preferences.EmailShipmentUpdates = preferences.EmailShipmentUpdates;
            Preferences.EmailPaymentReminders = preferences.EmailPaymentReminders;
            Preferences.EmailWarehouseAlerts = preferences.EmailWarehouseAlerts;
            Preferences.EmailCustomerInteractions = preferences.EmailCustomerInteractions;
            Preferences.EmailSystemAnnouncements = preferences.EmailSystemAnnouncements;
            Preferences.InAppShipmentUpdates = preferences.InAppShipmentUpdates;
            Preferences.InAppPaymentReminders = preferences.InAppPaymentReminders;
            Preferences.InAppWarehouseAlerts = preferences.InAppWarehouseAlerts;
            Preferences.InAppCustomerInteractions = preferences.InAppCustomerInteractions;
            Preferences.InAppSystemAnnouncements = preferences.InAppSystemAnnouncements;
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
            var request = new UpdateNotificationPreferencesRequest(
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

            await _notificationService.UpdatePreferencesAsync(user.Id, request, cancellationToken);

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
