using Ambev.DeveloperEvaluation.Application.Common.Querying;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Users;

/// <summary>
/// Fields of the user resource that can be used in <c>_order</c> and filters.
/// </summary>
public static class UserQueryFields
{
    public static readonly QueryFieldMap<User> Map = new(
        new Dictionary<string, string>
        {
            ["id"] = nameof(User.Id),
            ["email"] = nameof(User.Email),
            ["username"] = nameof(User.Username),
            ["phone"] = nameof(User.Phone),
            ["status"] = nameof(User.Status),
            ["role"] = nameof(User.Role),
            ["name.firstname"] = "Name.Firstname",
            ["name.lastname"] = "Name.Lastname",
            ["address.city"] = "Address.City",
            ["address.street"] = "Address.Street",
            ["address.number"] = "Address.Number",
            ["address.zipcode"] = "Address.Zipcode"
        },
        new OrderCriterion(nameof(User.Id), Descending: false));
}
