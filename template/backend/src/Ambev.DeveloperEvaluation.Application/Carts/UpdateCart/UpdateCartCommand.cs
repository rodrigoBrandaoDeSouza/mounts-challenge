using MediatR;
using System.Text.Json.Serialization;

namespace Ambev.DeveloperEvaluation.Application.Carts.UpdateCart;

/// <summary>
/// Updates a cart. The informed products replace the current ones.
/// </summary>
public class UpdateCartCommand : CartInput, IRequest<CartResult>
{
    /// <summary>Identifier of the cart (taken from the route).</summary>
    [JsonIgnore]
    public int Id { get; set; }
}
