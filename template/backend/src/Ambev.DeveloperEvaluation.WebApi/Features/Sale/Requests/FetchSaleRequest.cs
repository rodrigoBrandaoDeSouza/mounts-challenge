namespace Ambev.DeveloperEvaluation.WebApi.Features.Sale.Requests
{
    /// <summary>
    /// Represents a filtering parameters for fetching sales.
    /// </summary>
    public class FetchSalesRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
