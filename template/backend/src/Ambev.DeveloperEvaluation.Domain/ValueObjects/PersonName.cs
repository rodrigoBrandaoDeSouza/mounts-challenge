namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Name of a person (value object owned by <see cref="Entities.User"/>).
/// </summary>
public class PersonName
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}
