namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.Requests
{
    /// <summary>
    /// Represents the HTTP request to delete an existing sale.
    /// </summary>
    public class DeleteSaleRequest
    {
        public Guid Id { get; set; }
    }
}
