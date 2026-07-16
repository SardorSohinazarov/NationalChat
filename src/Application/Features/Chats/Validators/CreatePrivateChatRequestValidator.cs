using Application.Features.Chats.DataTransferObjects.Requests;
using FluentValidation;

namespace Application.Features.Chats.Validators;

public sealed class CreatePrivateChatRequestValidator : AbstractValidator<CreatePrivateChatRequest>
{
    public CreatePrivateChatRequestValidator() =>
        RuleFor(x => x.UserId).GreaterThan(0);
}
