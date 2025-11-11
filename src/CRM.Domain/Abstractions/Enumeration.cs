namespace CRM.Domain.Abstractions;

public abstract class Enumeration : IComparable
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        return Id.Equals(otherValue.Id) && Name.Equals(otherValue.Name, StringComparison.Ordinal);
    }

    public override int GetHashCode() => HashCode.Combine(Id, Name);

    public int CompareTo(object? other) =>
        other is Enumeration enumeration ? Id.CompareTo(enumeration.Id) : -1;

    public static IEnumerable<T> GetAll<T>()
        where T : Enumeration
    {
        return typeof(T).GetFields(System.Reflection.BindingFlags.Public |
                                   System.Reflection.BindingFlags.Static |
                                   System.Reflection.BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(T))
            .Select(f => (T)f.GetValue(null)!);
    }
}

