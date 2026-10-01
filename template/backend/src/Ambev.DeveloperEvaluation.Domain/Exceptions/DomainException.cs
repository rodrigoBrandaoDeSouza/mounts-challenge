namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

/// <summary>
/// Raised when a business rule of the domain is violated.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
