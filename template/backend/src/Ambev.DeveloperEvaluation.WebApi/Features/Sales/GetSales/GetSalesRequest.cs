namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

public class GetSalesRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
