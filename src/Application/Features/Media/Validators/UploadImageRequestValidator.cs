using FluentValidation;
using Application.Features.Media.DataTransferObjects.Requests;

namespace Application.Features.Media.Validators;

public sealed class UploadImageRequestValidator : AbstractValidator<UploadImageRequest>
{
    public UploadImageRequestValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Content.Length).LessThanOrEqualTo(10 * 1024 * 1024)
            .WithMessage("Rasm hajmi 10 MB dan oshmasligi kerak.");
        RuleFor(x => x.TextContent).MaximumLength(4_000).When(x => x.TextContent is not null);
        RuleFor(x => x.ReplyToMessageId).GreaterThan(0).When(x => x.ReplyToMessageId.HasValue);
    }
}
