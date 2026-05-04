using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CreateSaleHandler>>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _logger);
    }

    [Fact(DisplayName = "Given valid command When creating sale Then returns success result")]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Given
        var command = SaleTestData.GenerateValidCommand();
        var sale = SaleTestData.GenerateSaleWithItems();
        var expectedResult = new CreateSaleResult { Id = sale.Id, SaleNumber = sale.SaleNumber };

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(expectedResult);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedResult.Id);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid command When creating sale Then throws ValidationException")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand(); // empty — will fail validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given command with quantity above 20 When creating sale Then throws DomainException")]
    public async Task Handle_QuantityAbove20_ThrowsDomainException()
    {
        // Given
        var command = SaleTestData.GenerateValidCommand();
        command.Items = new List<CreateSaleItemCommand>
        {
            new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Product X",
                Quantity = 21,
                UnitPrice = 10m
            }
        };

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*20*");
    }

    [Fact(DisplayName = "Given valid command When creating sale Then discounts are applied per business rules")]
    public async Task Handle_ValidCommand_AppliesDiscountsCorrectly()
    {
        // Given
        var command = SaleTestData.GenerateValidCommand();
        command.Items = new List<CreateSaleItemCommand>
        {
            new CreateSaleItemCommand { ProductId = Guid.NewGuid(), ProductName = "A", Quantity = 4,  UnitPrice = 100m },
            new CreateSaleItemCommand { ProductId = Guid.NewGuid(), ProductName = "B", Quantity = 10, UnitPrice = 100m },
            new CreateSaleItemCommand { ProductId = Guid.NewGuid(), ProductName = "C", Quantity = 2,  UnitPrice = 100m },
        };

        Sale? capturedSale = null;
        _saleRepository.CreateAsync(Arg.Do<Sale>(s => capturedSale = s), Arg.Any<CancellationToken>())
            .Returns(x => (Sale)x[0]);
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(new CreateSaleResult());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        capturedSale.Should().NotBeNull();
        capturedSale!.Items.First(i => i.Quantity == 4).Discount.Should().Be(0.10m);
        capturedSale!.Items.First(i => i.Quantity == 10).Discount.Should().Be(0.20m);
        capturedSale!.Items.First(i => i.Quantity == 2).Discount.Should().Be(0.00m);
    }
}
