namespace CRM.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when the request is well-formed but contains semantic errors.
/// Maps to HTTP 422 Unprocessable Entity status code.
/// </summary>
public class UnprocessableEntityException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public UnprocessableEntityException()
        : base("The request is well-formed but contains semantic errors.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public UnprocessableEntityException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public UnprocessableEntityException(string message, IDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public UnprocessableEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }
}

