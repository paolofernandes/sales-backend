using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Theory(DisplayName = "Given quantity 1-3 When applying discount Then no discount is applied")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ApplyDiscount_Quantity1to3_NoDiscount(int quantity)
    {
        // Given
        var item = SaleTestData.GenerateSaleItem(quantity, 100m);

        // When
        item.ApplyDiscount();

        // Then
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(quantity * 100m);
    }

    [Theory(DisplayName = "Given quantity 4-9 When applying discount Then 10% discount is applied")]
    [InlineData(4)]
    [InlineData(7)]
    [InlineData(9)]
    public void ApplyDiscount_Quantity4to9_10PercentDiscount(int quantity)
    {
        // Given
        var item = SaleTestData.GenerateSaleItem(quantity, 100m);

        // When
        item.ApplyDiscount();

        // Then
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(quantity * 100m * 0.90m);
    }

    [Theory(DisplayName = "Given quantity 10-20 When applying discount Then 20% discount is applied")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void ApplyDiscount_Quantity10to20_20PercentDiscount(int quantity)
    {
        // Given
        var item = SaleTestData.GenerateSaleItem(quantity, 100m);

        // When
        item.ApplyDiscount();

        // Then
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(quantity * 100m * 0.80m);
    }

    [Theory(DisplayName = "Given quantity above 20 When applying discount Then throws DomainException")]
    [InlineData(21)]
    [InlineData(50)]
    public void ApplyDiscount_QuantityAbove20_ThrowsDomainException(int quantity)
    {
        // Given
        var item = SaleTestData.GenerateSaleItem(quantity, 100m);

        // When
        var act = () => item.ApplyDiscount();

        // Then
        act.Should().Throw<DomainException>()
            .WithMessage("*20*");
    }

    [Fact(DisplayName = "Given active sale When cancelling Then IsCancelled is set to true")]
    public void Cancel_SetsCancelledFlag()
    {
        // Given
        var sale = SaleTestData.GenerateSaleWithItems();

        // When
        sale.Cancel();

        // Then
        sale.IsCancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given sale with items When cancelling one item Then item is cancelled and total is recalculated")]
    public void CancelItem_SetsItemCancelledAndRecalculatesTotal()
    {
        // Given
        var item1 = SaleTestData.GenerateSaleItem(2, 100m);
        var item2 = SaleTestData.GenerateSaleItem(3, 50m);
        item1.ApplyDiscount();
        item2.ApplyDiscount();

        var sale = SaleTestData.GenerateSaleWithItems(new List<SaleItem> { item1, item2 });
        var expectedTotalAfterCancel = item2.TotalAmount;

        // When
        sale.CancelItem(item1.Id);

        // Then
        item1.IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(expectedTotalAfterCancel);
    }

    [Fact(DisplayName = "Given sale with items When cancelling non-existent item Then throws KeyNotFoundException")]
    public void CancelItem_NonExistentItem_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = SaleTestData.GenerateSaleWithItems();

        // When
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Then
        act.Should().Throw<KeyNotFoundException>();
    }
}
