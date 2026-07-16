using Application.DataTransferObjects.Pagination;
using Application.Features.Contacts.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Contacts.Validators;

public sealed class AddContactRequestValidator : AbstractValidator<AddContactRequest>
{
    public AddContactRequestValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Username yoki email kiritilishi kerak.")
            .MaximumLength(254).WithMessage("Username yoki email 254 belgidan oshmasligi kerak.");

        RuleFor(x => x.CustomFirstName)
            .MaximumLength(100).WithMessage("Kontakt ismi 100 belgidan oshmasligi kerak.");
        RuleFor(x => x.CustomLastName)
            .MaximumLength(100).WithMessage("Kontakt familiyasi 100 belgidan oshmasligi kerak.");
    }
}

public sealed class CursorPaginationRequestValidator : AbstractValidator<CursorPaginationRequest>
{
    public CursorPaginationRequestValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
        RuleFor(x => x.BeforeId).GreaterThan(0).When(x => x.BeforeId.HasValue);
    }
}
