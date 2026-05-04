using AutoMapper;
using FluentValidation;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesHandler : IRequestHandler<GetSalesQuery, GetSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<GetSalesResult> Handle(GetSalesQuery query, CancellationToken cancellationToken)
    {
        var validator = new GetSalesQueryValidator();
        var validationResult = await validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (items, totalCount) = await _saleRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

        return new GetSalesResult
        {
            Items = _mapper.Map<IEnumerable<GetSalesSaleResult>>(items),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }
}
