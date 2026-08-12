using Application.Features.Messages.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Messages.Validators;

public sealed class MessageSearchRequestValidator : AbstractValidator<MessageSearchRequest>
{
    public MessageSearchRequestValidator()
    {
        RuleFor(x => x.Query)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Qidiruv matni kiritilishi kerak.")
            .MaximumLength(100).WithMessage("Qidiruv matni 100 belgidan oshmasligi kerak.");
        RuleFor(x => x.Limit).InclusiveBetween(1, 50);
    }
}
