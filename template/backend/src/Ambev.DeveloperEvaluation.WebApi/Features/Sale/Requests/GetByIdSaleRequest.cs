namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.Requests
{
    /// <summary>
    /// Represents the HTTP request to retrieve a sale by its unique identifier.
    /// </summary>
    public class GetSaleByIdRequest
    {
        public Guid Id { get; set; }
    }
}
