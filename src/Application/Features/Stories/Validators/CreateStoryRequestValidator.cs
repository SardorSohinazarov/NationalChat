using Application.Features.Stories.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Stories.Validators;

public sealed class CreateStoryRequestValidator : AbstractValidator<CreateStoryRequest>
{
    public CreateStoryRequestValidator()
    {
        RuleFor(x => x.FileName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Fayl tanlanmagan.");
        RuleFor(x => x.Content)
            .Must(x => x.Length > 0).WithMessage("Fayl tanlanmagan.");
        RuleFor(x => x.Caption)
            .MaximumLength(200).WithMessage("Izoh 200 belgidan oshmasligi kerak.");
    }
}
