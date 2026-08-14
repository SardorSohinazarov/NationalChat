using Application.Features.Messages.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Messages.Validators;

public sealed class SendFileAttachmentRequestValidator : AbstractValidator<SendFileAttachmentRequest>
{
    private const int MaxFileSizeBytes = 20 * 1024 * 1024;

    public SendFileAttachmentRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("Fayl tanlanmagan.")
            .MaximumLength(255).WithMessage("Fayl nomi 255 belgidan oshmasligi kerak.");
        RuleFor(x => x.Content)
            .Must(x => x.Length > 0).WithMessage("Fayl tanlanmagan.")
            .Must(x => x.Length <= MaxFileSizeBytes).WithMessage("Fayl hajmi 20 MB dan oshmasligi kerak.");
        RuleFor(x => x.TextContent)
            .MaximumLength(4_000).WithMessage("Xabar 4000 belgidan oshmasligi kerak.");
        RuleFor(x => x.ReplyToMessageId).GreaterThan(0).When(x => x.ReplyToMessageId.HasValue);
    }
}
