using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Application.Users;

/// <summary>
/// User returned by the API. The password (hash) is never returned.
/// </summary>
public class UserResult
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserNameDto Name { get; set; } = new();
    public UserAddressDto Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public UserRole Role { get; set; }
}

public class UserNameDto
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}

public class UserAddressDto
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Zipcode { get; set; } = string.Empty;
    public GeolocationDto Geolocation { get; set; } = new();
}

public class GeolocationDto
{
    public string Lat { get; set; } = string.Empty;
    public string Long { get; set; } = string.Empty;
}

/// <summary>
/// Data of a user informed on create/update.
/// </summary>
public abstract class UserInput
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserNameDto Name { get; set; } = new();
    public UserAddressDto Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;

    /// <summary>Status of the user (Active, Inactive, Suspended). Defaults to Active on create.</summary>
    public UserStatus? Status { get; set; }

    /// <summary>Role of the user (Customer, Manager, Admin). Defaults to Customer on create.</summary>
    public UserRole? Role { get; set; }
}
