namespace CRM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a request conflicts with the current state of the resource.
/// Maps to HTTP 409 Conflict status code.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException()
        : base("The request conflicts with the current state of the resource.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ConflictException(string resource, string reason)
        : base($"Conflict occurred with {resource}: {reason}")
    {
    }
}

