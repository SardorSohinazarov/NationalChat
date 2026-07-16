using Application.Features.Profiles.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Profiles.Validators;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Username)
            .Matches("^[a-zA-Z0-9_]{5,50}$")
            .WithMessage("Username 5-50 belgidan iborat bo'lib, faqat harf, raqam va '_' ishlatishi mumkin.");
        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ism kiritilishi kerak.")
            .MaximumLength(100).WithMessage("Ism 100 belgidan oshmasligi kerak.");
        RuleFor(x => x.LastName).MaximumLength(100).WithMessage("Familiya 100 belgidan oshmasligi kerak.");
        RuleFor(x => x.Bio).MaximumLength(255).WithMessage("Bio 255 belgidan oshmasligi kerak.");
    }
}
