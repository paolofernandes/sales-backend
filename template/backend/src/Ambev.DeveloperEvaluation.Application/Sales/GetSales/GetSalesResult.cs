namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesResult
{
    public IEnumerable<GetSalesSaleResult> Items { get; set; } = new List<GetSalesSaleResult>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class GetSalesSaleResult
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
