using Application.Features.Messages.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Messages.Validators;

public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.TextContent)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Xabar matni kiritilishi kerak.")
            .MaximumLength(4_000).WithMessage("Xabar 4000 belgidan oshmasligi kerak.");
        RuleFor(x => x.ReplyToMessageId).GreaterThan(0).When(x => x.ReplyToMessageId.HasValue);
    }
}
