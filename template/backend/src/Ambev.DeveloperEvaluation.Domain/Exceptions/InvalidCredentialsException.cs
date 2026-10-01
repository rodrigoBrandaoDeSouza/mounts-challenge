namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

/// <summary>
/// Raised when an authentication attempt fails.
/// </summary>
public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException(string message) : base(message)
    {
    }
}
