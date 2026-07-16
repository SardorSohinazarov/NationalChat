using Application.Features.Authentication.DataTransferObjects.Commands;
using Application.Features.Authentication.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Authentication.Validators;

public sealed class RequestSignInCodeCommandValidator : AbstractValidator<RequestSignInCodeCommand>
{
    public RequestSignInCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email manzili kiritilishi kerak.")
            .EmailAddress().WithMessage("Email manzili noto'g'ri.")
            .Must(AuthValidationRules.IsValidEmail).WithMessage("Email manzili noto'g'ri.");
    }
}

public sealed class RequestCodeRequestValidator : AbstractValidator<RequestCodeRequest>
{
    public RequestCodeRequestValidator() =>
        RuleFor(x => x.Email).SetValidator(new EmailValidator());
}

public sealed class VerifySignInCodeCommandValidator : AbstractValidator<VerifySignInCodeCommand>
{
    public VerifySignInCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email manzili kiritilishi kerak.")
            .EmailAddress().WithMessage("Email manzili noto'g'ri.")
            .Must(AuthValidationRules.IsValidEmail).WithMessage("Email manzili noto'g'ri.");

        RuleFor(x => x.Code)
            .Matches("^\\d{6}$").WithMessage("Tasdiqlash kodi 6 xonali bo'lishi kerak.");
    }
}

public sealed class VerifyCodeRequestValidator : AbstractValidator<VerifyCodeRequest>
{
    public VerifyCodeRequestValidator()
    {
        RuleFor(x => x.Email).SetValidator(new EmailValidator());
        RuleFor(x => x.Code).Matches("^\\d{6}$").WithMessage("Tasdiqlash kodi 6 xonali bo'lishi kerak.");
    }
}

public sealed class CompleteRegistrationCommandValidator : AbstractValidator<CompleteRegistrationCommand>
{
    public CompleteRegistrationCommandValidator()
    {
        RuleFor(x => x.RegistrationToken)
            .NotEmpty().WithMessage("Ro'yxatdan o'tish tokeni kiritilishi kerak.");

        RuleFor(x => x.Username)
            .Matches("^[a-zA-Z0-9_]{5,50}$").WithMessage("Username 5-50 belgidan iborat bo'lib, faqat harf, raqam va '_' ishlatishi mumkin.");

        RuleFor(x => x.FirstName)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ism kiritilishi kerak.")
            .MaximumLength(100).WithMessage("Ism 100 belgidan oshmasligi kerak.");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Familiya 100 belgidan oshmasligi kerak.")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));
    }
}

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.RegistrationToken).NotEmpty().WithMessage("Ro'yxatdan o'tish tokeni kiritilishi kerak.");
        RuleFor(x => x.Username).Matches("^[a-zA-Z0-9_]{5,50}$").WithMessage("Username 5-50 belgidan iborat bo'lib, faqat harf, raqam va '_' ishlatishi mumkin.");
        RuleFor(x => x.FirstName).Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Ism kiritilishi kerak.").MaximumLength(100).WithMessage("Ism 100 belgidan oshmasligi kerak.");
        RuleFor(x => x.LastName).MaximumLength(100).WithMessage("Familiya 100 belgidan oshmasligi kerak.").When(x => !string.IsNullOrWhiteSpace(x.LastName));
    }
}

internal static class AuthValidationRules
{
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var parsed = new System.Net.Mail.MailAddress(email.Trim());
            return string.Equals(parsed.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

internal sealed class EmailValidator : AbstractValidator<string>
{
    public EmailValidator() =>
        RuleFor(x => x)
            .NotEmpty().WithMessage("Email manzili kiritilishi kerak.")
            .EmailAddress().WithMessage("Email manzili noto'g'ri.")
            .Must(AuthValidationRules.IsValidEmail).WithMessage("Email manzili noto'g'ri.");
}

public sealed class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionCommandValidator() =>
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token topilmadi.");
}
