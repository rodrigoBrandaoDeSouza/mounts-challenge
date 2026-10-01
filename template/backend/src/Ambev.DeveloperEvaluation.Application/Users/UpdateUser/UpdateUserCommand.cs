using MediatR;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.Application.Users.UpdateUser;

/// <summary>
/// Updates a user. When the password is omitted, the current one is kept.
/// </summary>
public class UpdateUserCommand : UserInput, IRequest<UserResult>
{
    /// <summary>Identifier of the user (taken from the route).</summary>
    [JsonIgnore]
    public int Id { get; set; }
}
