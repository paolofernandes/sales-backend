namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;

public class GetSaleRequest
{
    public Guid Id { get; set; }
    public bool IncludeCancelledItems { get; set; } = false;
}
