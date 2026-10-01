namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

/// <summary>
/// Raised when an operation conflicts with the current state of a resource (e.g. a duplicated unique value).
/// </summary>
public class ConflictException : DomainException
{
    public string Error { get; }
    public string Detail { get; }

    public ConflictException(string error, string detail) : base(detail)
    {
        Error = error;
        Detail = detail;
    }
}
