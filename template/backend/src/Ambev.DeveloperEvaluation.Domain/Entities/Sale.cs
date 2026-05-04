using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<SaleItem> Items { get; set; } = new();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateItems(List<SaleItem> items)
    {
        Items = items;
        CalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public SaleItem CancelItem(Guid itemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} not found in sale {Id}");

        item.IsCancelled = true;
        CalculateTotal();
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    private void CalculateTotal()
    {
        TotalAmount = Items
            .Where(i => !i.IsCancelled)
            .Sum(i => i.TotalAmount);
    }
}
