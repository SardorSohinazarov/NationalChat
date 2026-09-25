using Application.Features.Users.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Users.Validators;

public sealed class UserSearchRequestValidator : AbstractValidator<UserSearchRequest>
{
    public UserSearchRequestValidator()
    {
        RuleFor(x => x.Query)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Qidiruv matni kiritilishi kerak.")
            .MaximumLength(50).WithMessage("Qidiruv matni 50 belgidan oshmasligi kerak.");
        RuleFor(x => x.Limit).InclusiveBetween(1, 50);
    }
}
