using System.ComponentModel.DataAnnotations;
using CRM.Domain.Customers;
using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly CRMDbContext _dbContext;

    public CustomersController(CRMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers(CancellationToken cancellationToken, [FromQuery] string? search = null)
    {
        var query = _dbContext.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                EF.Functions.Like(c.Name, $"%{search}%") ||
                EF.Functions.Like(c.LegalName!, $"%{search}%") ||
                EF.Functions.Like(c.TaxNumber!, $"%{search}%"));
        }

        var customers = await query
            .Select(c => new CustomerDto(
                c.Id,
                c.Name,
                c.LegalName,
                c.TaxNumber,
                c.Email,
                c.Phone,
                c.Address,
                c.Segment,
                c.Notes))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDetailsDto>> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            return NotFound();
        }

        var dto = new CustomerDetailsDto(
            customer.Id,
            customer.Name,
            customer.LegalName,
            customer.TaxNumber,
            customer.Email,
            customer.Phone,
            customer.Address,
            customer.Segment,
            customer.Notes,
            customer.Contacts
                .Select(contact => new CustomerContactDto(contact.Id, contact.FullName, contact.Email, contact.Phone, contact.Position))
                .ToList());

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDetailsDto>> CreateCustomer([FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var customer = new Customer(
            Guid.NewGuid(),
            request.Name,
            request.LegalName,
            request.TaxNumber,
            request.Email,
            request.Phone,
            request.Address,
            request.Segment,
            request.Notes);

        customer.ClearContacts();
        if (request.Contacts?.Any() == true)
        {
            foreach (var contact in request.Contacts)
            {
                customer.AddContact(contact.FullName, contact.Email, contact.Phone, contact.Position);
            }
        }

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, await BuildDetailsDto(customer.Id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDetailsDto>> UpdateCustomer(Guid id, [FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var customer = await _dbContext.Customers
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            return NotFound();
        }

        customer.Update(
            request.Name,
            request.LegalName,
            request.TaxNumber,
            request.Email,
            request.Phone,
            request.Address,
            request.Segment,
            request.Notes);

        customer.ClearContacts();
        if (request.Contacts?.Any() == true)
        {
            foreach (var contact in request.Contacts)
            {
                customer.AddContact(contact.FullName, contact.Email, contact.Phone, contact.Position);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(await BuildDetailsDto(customer.Id, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<CustomerDetailsDto> BuildDetailsDto(Guid id, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .AsNoTracking()
            .Include(c => c.Contacts)
            .FirstAsync(c => c.Id == id, cancellationToken);

        return new CustomerDetailsDto(
            customer.Id,
            customer.Name,
            customer.LegalName,
            customer.TaxNumber,
            customer.Email,
            customer.Phone,
            customer.Address,
            customer.Segment,
            customer.Notes,
            customer.Contacts
                .Select(contact => new CustomerContactDto(contact.Id, contact.FullName, contact.Email, contact.Phone, contact.Position))
                .ToList());
    }

    public sealed record CustomerDto(
        Guid Id,
        string Name,
        string? LegalName,
        string? TaxNumber,
        string? Email,
        string? Phone,
        string? Address,
        string? Segment,
        string? Notes);

    public sealed record CustomerDetailsDto(
        Guid Id,
        string Name,
        string? LegalName,
        string? TaxNumber,
        string? Email,
        string? Phone,
        string? Address,
        string? Segment,
        string? Notes,
        IReadOnlyCollection<CustomerContactDto> Contacts);

    public sealed record CustomerContactDto(Guid Id, string FullName, string? Email, string? Phone, string? Position);

    public sealed class CustomerRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? LegalName { get; set; }

        [MaxLength(50)]
        public string? TaxNumber { get; set; }

        [EmailAddress]
        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Segment { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public List<CustomerContactRequest>? Contacts { get; set; }
    }

    public sealed class CustomerContactRequest
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Position { get; set; }
    }
}

