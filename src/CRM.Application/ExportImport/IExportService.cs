namespace CRM.Application.ExportImport;

public interface IExportService
{
    /// <summary>
    /// Customers listesini Excel formatında export eder
    /// </summary>
    Task<byte[]> ExportCustomersToExcelAsync(IReadOnlyList<object> customers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suppliers listesini Excel formatında export eder
    /// </summary>
    Task<byte[]> ExportSuppliersToExcelAsync(IReadOnlyList<object> suppliers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shipments listesini Excel formatında export eder
    /// </summary>
    Task<byte[]> ExportShipmentsToExcelAsync(IReadOnlyList<object> shipments, CancellationToken cancellationToken = default);

    /// <summary>
    /// Customers listesini CSV formatında export eder
    /// </summary>
    Task<byte[]> ExportCustomersToCsvAsync(IReadOnlyList<object> customers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suppliers listesini CSV formatında export eder
    /// </summary>
    Task<byte[]> ExportSuppliersToCsvAsync(IReadOnlyList<object> suppliers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Shipments listesini CSV formatında export eder
    /// </summary>
    Task<byte[]> ExportShipmentsToCsvAsync(IReadOnlyList<object> shipments, CancellationToken cancellationToken = default);
}

