namespace CRM.Domain.Enums;

public enum ShipmentStatus
{
    Ordered = 0,
    ProductionStarted = 1,
    Departed = 2,
    OnTrain = 3,
    OnVessel = 4,
    AtPort = 5,
    InCustoms = 6,
    OnTruck = 7,
    InWarehouse = 8,
    DeliveredToDealer = 9,
    Cancelled = 10
}

