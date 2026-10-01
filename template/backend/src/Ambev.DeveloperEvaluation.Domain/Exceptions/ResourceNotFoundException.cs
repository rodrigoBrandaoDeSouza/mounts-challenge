namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

/// <summary>
/// Raised when a requested resource does not exist.
/// </summary>
public class ResourceNotFoundException : DomainException
{
    /// <summary>Short, human-readable summary of the problem (e.g. "Product not found").</summary>
    public string Error { get; }

    /// <summary>Explanation specific to this occurrence of the problem.</summary>
    public string Detail { get; }

    public ResourceNotFoundException(string error, string detail) : base(detail)
    {
        Error = error;
        Detail = detail;
    }

    /// <summary>Creates the standard "&lt;resource&gt; not found" exception.</summary>
    public static ResourceNotFoundException For(string resource, object id) =>
        new($"{resource} not found", $"The {resource.ToLowerInvariant()} with ID {id} does not exist in our database");
}
