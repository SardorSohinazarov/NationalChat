using Application.Features.Messages.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Messages.Validators;

public sealed class UpdateMessageRequestValidator : AbstractValidator<UpdateMessageRequest>
{
    public UpdateMessageRequestValidator()
    {
        RuleFor(x => x.TextContent)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Xabar matni kiritilishi kerak.")
            .MaximumLength(4_000).WithMessage("Xabar 4000 belgidan oshmasligi kerak.");
    }
}
