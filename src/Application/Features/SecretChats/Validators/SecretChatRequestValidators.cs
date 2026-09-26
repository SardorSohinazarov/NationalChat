using Application.Features.SecretChats.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.SecretChats.Validators;

public sealed class CreateSecretChatRequestValidator : AbstractValidator<CreateSecretChatRequest>
{
    public CreateSecretChatRequestValidator()
    {
        RuleFor(x => x.ParticipantId).GreaterThan(0).WithMessage("Suhbatdosh tanlanishi kerak.");
        RuleFor(x => x.PublicKey)
            .Must(SecretChatBase64.IsPublicKey).WithMessage("Ochiq kalit noto'g'ri.");
    }
}

public sealed class AcceptSecretChatRequestValidator : AbstractValidator<AcceptSecretChatRequest>
{
    public AcceptSecretChatRequestValidator()
    {
        RuleFor(x => x.PublicKey)
            .Must(SecretChatBase64.IsPublicKey).WithMessage("Ochiq kalit noto'g'ri.");
    }
}

public sealed class SendSecretMessageRequestValidator : AbstractValidator<SendSecretMessageRequest>
{
    public SendSecretMessageRequestValidator()
    {
        RuleFor(x => x.Seq).GreaterThan(0).WithMessage("Xabar tartib raqami noto'g'ri.");
        RuleFor(x => x.Ciphertext)
            .Must(value => SecretChatBase64.TryDecode(value) is { Length: > 0 and <= SecretChatLimits.MaxCiphertextBytes })
            .WithMessage($"Shifrlangan xabar bo'sh yoki {SecretChatLimits.MaxCiphertextBytes / 1024} KB dan katta.");
    }
}

public sealed class AckSecretMessagesRequestValidator : AbstractValidator<AckSecretMessagesRequest>
{
    public AckSecretMessagesRequestValidator()
    {
        RuleFor(x => x.UpToId).GreaterThan(0);
    }
}

public sealed class SecretMessagesQueryValidator : AbstractValidator<SecretMessagesQuery>
{
    public SecretMessagesQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, SecretChatLimits.MaxPageSize);
        RuleFor(x => x.AfterId).GreaterThan(0).When(x => x.AfterId.HasValue);
    }
}
