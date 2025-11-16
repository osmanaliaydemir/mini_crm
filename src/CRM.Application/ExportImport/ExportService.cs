using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using CRM.Application.Customers;
using CRM.Application.Suppliers;
using CRM.Application.Shipments;

namespace CRM.Application.ExportImport;

public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<byte[]> ExportCustomersToExcelAsync(IReadOnlyList<object> customers, CancellationToken cancellationToken = default)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Müşteriler");

        // Headers
        var headers = new[] { "Ad", "Yasal Ad", "Segment", "E-posta", "Telefon", "Notlar" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        int row = 2;
        foreach (var customer in customers)
        {
            if (customer is CustomerListItemDto dto)
            {
                worksheet.Cells[row, 1].Value = dto.Name;
                worksheet.Cells[row, 2].Value = dto.LegalName ?? string.Empty;
                worksheet.Cells[row, 3].Value = dto.Segment ?? string.Empty;
                worksheet.Cells[row, 4].Value = dto.Email ?? string.Empty;
                worksheet.Cells[row, 5].Value = dto.Phone ?? string.Empty;
                worksheet.Cells[row, 6].Value = dto.Notes ?? string.Empty;
                row++;
            }
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExportSuppliersToExcelAsync(IReadOnlyList<object> suppliers, CancellationToken cancellationToken = default)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Tedarikçiler");

        // Headers
        var headers = new[] { "Ad", "Ülke", "Vergi Numarası", "E-posta", "Telefon", "Adres", "Notlar" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        int row = 2;
        foreach (var supplier in suppliers)
        {
            if (supplier is SupplierDto dto)
            {
                worksheet.Cells[row, 1].Value = dto.Name;
                worksheet.Cells[row, 2].Value = dto.Country ?? string.Empty;
                worksheet.Cells[row, 3].Value = dto.TaxNumber ?? string.Empty;
                worksheet.Cells[row, 4].Value = dto.ContactEmail ?? string.Empty;
                worksheet.Cells[row, 5].Value = dto.ContactPhone ?? string.Empty;
                worksheet.Cells[row, 6].Value = dto.AddressLine ?? string.Empty;
                worksheet.Cells[row, 7].Value = dto.Notes ?? string.Empty;
                row++;
            }
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExportShipmentsToExcelAsync(IReadOnlyList<object> shipments, CancellationToken cancellationToken = default)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Sevkiyatlar");

        // Headers
        var headers = new[] { "Referans No", "Tedarikçi", "Müşteri", "Durum", "Sevkiyat Tarihi", "Tahmini Varış", "Son Güncelleme", "Notlar" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // Data
        int row = 2;
        foreach (var shipment in shipments)
        {
            if (shipment is ShipmentListItemDto dto)
            {
                worksheet.Cells[row, 1].Value = dto.ReferenceNumber;
                worksheet.Cells[row, 2].Value = dto.SupplierName;
                worksheet.Cells[row, 3].Value = dto.CustomerName;
                worksheet.Cells[row, 4].Value = dto.Status.ToString();
                worksheet.Cells[row, 5].Value = dto.ShipmentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                worksheet.Cells[row, 6].Value = dto.EstimatedArrival?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
                worksheet.Cells[row, 7].Value = dto.LastStageUpdate?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty;
                worksheet.Cells[row, 8].Value = dto.LastStageNotes ?? string.Empty;
                row++;
            }
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return await Task.FromResult(package.GetAsByteArray());
    }

    public async Task<byte[]> ExportCustomersToCsvAsync(IReadOnlyList<object> customers, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        
        // Headers
        sb.AppendLine("Ad,Yasal Ad,Segment,E-posta,Telefon,Notlar");

        // Data
        foreach (var customer in customers)
        {
            if (customer is CustomerListItemDto dto)
            {
                sb.AppendLine($"{EscapeCsvField(dto.Name)},{EscapeCsvField(dto.LegalName ?? string.Empty)},{EscapeCsvField(dto.Segment ?? string.Empty)},{EscapeCsvField(dto.Email ?? string.Empty)},{EscapeCsvField(dto.Phone ?? string.Empty)},{EscapeCsvField(dto.Notes ?? string.Empty)}");
            }
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public async Task<byte[]> ExportSuppliersToCsvAsync(IReadOnlyList<object> suppliers, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        
        // Headers
        sb.AppendLine("Ad,Ülke,Vergi Numarası,E-posta,Telefon,Adres,Notlar");

        // Data
        foreach (var supplier in suppliers)
        {
            if (supplier is SupplierDto dto)
            {
                sb.AppendLine($"{EscapeCsvField(dto.Name)},{EscapeCsvField(dto.Country ?? string.Empty)},{EscapeCsvField(dto.TaxNumber ?? string.Empty)},{EscapeCsvField(dto.ContactEmail ?? string.Empty)},{EscapeCsvField(dto.ContactPhone ?? string.Empty)},{EscapeCsvField(dto.AddressLine ?? string.Empty)},{EscapeCsvField(dto.Notes ?? string.Empty)}");
            }
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public async Task<byte[]> ExportShipmentsToCsvAsync(IReadOnlyList<object> shipments, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        
        // Headers
        sb.AppendLine("Referans No,Tedarikçi,Müşteri,Durum,Sevkiyat Tarihi,Tahmini Varış,Son Güncelleme,Notlar");

        // Data
        foreach (var shipment in shipments)
        {
            if (shipment is ShipmentListItemDto dto)
            {
                sb.AppendLine($"{EscapeCsvField(dto.ReferenceNumber)},{EscapeCsvField(dto.SupplierName)},{EscapeCsvField(dto.CustomerName)},{EscapeCsvField(dto.Status.ToString())},{EscapeCsvField(dto.ShipmentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))},{EscapeCsvField(dto.EstimatedArrival?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty)},{EscapeCsvField(dto.LastStageUpdate?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? string.Empty)},{EscapeCsvField(dto.LastStageNotes ?? string.Empty)}");
            }
        }

        return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}

