using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesQuery : IRequest<GetSalesResult>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
