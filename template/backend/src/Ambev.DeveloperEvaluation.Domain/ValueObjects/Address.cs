namespace Ambev.DeveloperEvaluation.Domain.ValueObjects;

/// <summary>
/// Postal address with geolocation (value object owned by <see cref="Entities.User"/>).
/// </summary>
public class Address
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Zipcode { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
}
