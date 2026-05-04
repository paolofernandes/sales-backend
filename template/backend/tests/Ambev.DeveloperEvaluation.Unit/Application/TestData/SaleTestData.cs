using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class SaleTestData
{
    private static readonly Faker<CreateSaleItemCommand> itemCommandFaker = new Faker<CreateSaleItemCommand>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid())
        .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, 3))
        .RuleFor(i => i.UnitPrice, f => f.Random.Decimal(1, 500));

    private static readonly Faker<CreateSaleCommand> createSaleCommandFaker = new Faker<CreateSaleCommand>()
        .RuleFor(c => c.SaleNumber, f => f.Random.AlphaNumeric(8).ToUpper())
        .RuleFor(c => c.SaleDate, f => f.Date.Recent().ToUniversalTime())
        .RuleFor(c => c.CustomerId, f => f.Random.Guid())
        .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
        .RuleFor(c => c.BranchId, f => f.Random.Guid())
        .RuleFor(c => c.BranchName, f => f.Address.City())
        .RuleFor(c => c.Items, f => itemCommandFaker.Generate(f.Random.Int(1, 3)));

    public static CreateSaleCommand GenerateValidCommand() => createSaleCommandFaker.Generate();

    public static SaleItem GenerateSaleItem(int quantity = 1, decimal unitPrice = 100m)
    {
        return new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = new Faker().Commerce.ProductName(),
            Quantity = quantity,
            UnitPrice = unitPrice,
        };
    }

    public static Sale GenerateSaleWithItems(List<SaleItem>? items = null)
    {
        var faker = new Faker();
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = faker.Random.AlphaNumeric(8).ToUpper(),
            SaleDate = faker.Date.Recent().ToUniversalTime(),
            CustomerId = faker.Random.Guid(),
            CustomerName = faker.Company.CompanyName(),
            BranchId = faker.Random.Guid(),
            BranchName = faker.Address.City(),
        };

        var saleItems = items ?? new List<SaleItem> { GenerateSaleItem(2, 100m) };
        foreach (var item in saleItems)
            item.SaleId = sale.Id;

        sale.UpdateItems(saleItems);
        return sale;
    }
}
