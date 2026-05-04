using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleItemHandler> _logger;

    public CancelSaleItemHandler(ISaleRepository saleRepository, ILogger<CancelSaleItemHandler> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found");

        var item = sale.CancelItem(command.ItemId);

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        _logger.LogInformation("Event: {EventName} | SaleId: {SaleId} | ItemId: {ItemId}",
            nameof(ItemCancelledEvent), sale.Id, item.Id);

        return new CancelSaleItemResult
        {
            SaleId = sale.Id,
            ItemId = item.Id,
            IsCancelled = item.IsCancelled,
            NewSaleTotal = sale.TotalAmount,
        };
    }
}
