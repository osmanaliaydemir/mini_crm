namespace CRM.Domain.Notifications;

public enum EmailResourceType
{
    Shipment = 1,
    Finance = 2,
    Task = 3,
    Customer = 4,
    Warehouse = 5
}

public enum EmailTriggerType
{
    ShipmentStatusChanged = 1,
    ShipmentNoteAdded = 2,
    FinanceSummaryScheduled = 3,
    TaskAssigned = 4,
    TaskCompleted = 5,
    CustomerCreated = 6,
    WarehouseCreated = 7
}

public enum EmailExecutionType
{
    EventBased = 1,
    Scheduled = 2
}

public enum EmailRecipientType
{
    CustomEmail = 1,
    User = 2,
    Role = 3
}

