namespace CRM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a user is authenticated but does not have permission to perform the requested action.
/// Maps to HTTP 403 Forbidden status code.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You do not have permission to perform this action.")
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }

    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ForbiddenException(string resource, string action)
        : base($"You do not have permission to {action} the {resource}.")
    {
    }
}

