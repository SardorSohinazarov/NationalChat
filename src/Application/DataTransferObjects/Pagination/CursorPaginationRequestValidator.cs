using FluentValidation;

namespace Application.DataTransferObjects.Pagination;

public sealed class CursorPaginationRequestValidator : AbstractValidator<CursorPaginationRequest>
{
    public CursorPaginationRequestValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
        RuleFor(x => x.BeforeId).GreaterThan(0).When(x => x.BeforeId.HasValue);
    }
}
