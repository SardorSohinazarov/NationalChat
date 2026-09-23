using Application.Features.Groups.DataTransferObjects.Requests;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Groups.Validators;

public sealed class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Title)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Guruh nomi kiritilishi kerak.")
            .MaximumLength(GroupLimits.TitleMaxLength).WithMessage($"Guruh nomi {GroupLimits.TitleMaxLength} belgidan oshmasligi kerak.");
        RuleFor(x => x.Description)
            .MaximumLength(GroupLimits.DescriptionMaxLength).WithMessage($"Tavsif {GroupLimits.DescriptionMaxLength} belgidan oshmasligi kerak.");
        RuleFor(x => x.MemberIds)
            .NotNull().WithMessage("Kamida bitta a'zo tanlanishi kerak.")
            .Must(ids => ids is { Count: > 0 and < GroupLimits.MaxMembers }).WithMessage($"A'zolar soni 1 dan {GroupLimits.MaxMembers - 1} gacha bo'lishi kerak.");
        RuleForEach(x => x.MemberIds).GreaterThan(0);
    }
}

public sealed class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
    public UpdateGroupRequestValidator()
    {
        RuleFor(x => x.Title)
            .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Guruh nomi kiritilishi kerak.")
            .MaximumLength(GroupLimits.TitleMaxLength).WithMessage($"Guruh nomi {GroupLimits.TitleMaxLength} belgidan oshmasligi kerak.");
        RuleFor(x => x.Description)
            .MaximumLength(GroupLimits.DescriptionMaxLength).WithMessage($"Tavsif {GroupLimits.DescriptionMaxLength} belgidan oshmasligi kerak.");
    }
}

public sealed class AddGroupMembersRequestValidator : AbstractValidator<AddGroupMembersRequest>
{
    public AddGroupMembersRequestValidator()
    {
        RuleFor(x => x.UserIds)
            .NotNull().WithMessage("Kamida bitta foydalanuvchi tanlanishi kerak.")
            .Must(ids => ids is { Count: > 0 and <= GroupLimits.MaxMembersPerRequest })
            .WithMessage($"Bir vaqtda 1 dan {GroupLimits.MaxMembersPerRequest} gacha foydalanuvchi qo'shish mumkin.");
        RuleForEach(x => x.UserIds).GreaterThan(0);
    }
}

public sealed class UpdateGroupMemberRoleRequestValidator : AbstractValidator<UpdateGroupMemberRoleRequest>
{
    public UpdateGroupMemberRoleRequestValidator() =>
        RuleFor(x => x.Role)
            .Must(role => role is ChatMemberRole.Member or ChatMemberRole.Admin)
            .WithMessage("Faqat a'zo yoki admin roli berilishi mumkin.");
}
