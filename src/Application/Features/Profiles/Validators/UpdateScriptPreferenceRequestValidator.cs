using Application.Features.Profiles.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Profiles.Validators;

public sealed class UpdateScriptPreferenceRequestValidator : AbstractValidator<UpdateScriptPreferenceRequest>
{
    public UpdateScriptPreferenceRequestValidator()
    {
        RuleFor(x => x.ScriptPreference)
            .IsInEnum()
            .WithMessage("Yozuv turi noto'g'ri: 1 (asl holida), 2 (lotin) yoki 3 (kirill) bo'lishi kerak.");
    }
}
