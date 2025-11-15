using CRM.Domain.Abstractions;
using CRM.Domain.Common.ValueObjects;

namespace CRM.Domain.Products;

public class LumberVariant : Entity<Guid>, IAuditableEntity
{
    private LumberVariant()
    {
    }

    public LumberVariant(Guid id, string name, string? species, string? grade, Measurement? standardVolume, string unitOfMeasure)
    {
        Id = id;
        Name = name;
        Species = species;
        Grade = grade;
        StandardVolume = standardVolume;
        UnitOfMeasure = unitOfMeasure;
        CreatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Species { get; private set; }
    public string? Grade { get; private set; }
    public Measurement? StandardVolume { get; private set; }
    public string UnitOfMeasure { get; private set; } = "m3";
    public string? Notes { get; private set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void Update(string name, string? species, string? grade, Measurement? standardVolume, string unitOfMeasure, string? notes)
    {
        Name = name;
        Species = species;
        Grade = grade;
        StandardVolume = standardVolume;
        UnitOfMeasure = unitOfMeasure;
        Notes = notes;
        LastModifiedAt = DateTime.UtcNow;
    }
}

