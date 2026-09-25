using Application.Features.Files;
using Application.Features.Files.DataTransferObjects.Requests;
using Application.Features.Groups.DataTransferObjects.Requests;
using Application.Features.Groups.DataTransferObjects.Responses;
using Application.Features.Groups.Factories;
using Application.Features.Groups.Mappers;
using Application.Features.Messages;
using Application.Features.Messages.Factories;
using Application.Features.Messages.Mappers;
using Application.Features.Presence;
using Domain.Entities;
using FluentValidation;

namespace Application.Features.Groups;

public sealed class GroupService(
    IGroupRepository repository,
    IFileService fileService,
    IChatRealtimeNotifier realtimeNotifier,
    IPresenceTracker presenceTracker,
    IValidator<CreateGroupRequest> createValidator,
    IValidator<UpdateGroupRequest> updateValidator,
    IValidator<AddGroupMembersRequest> addMembersValidator,
    IValidator<UpdateGroupMemberRoleRequest> updateRoleValidator,
    TimeProvider timeProvider) : IGroupService
{
    private const string NotFoundError = "Guruh topilmadi.";
    private const string ForbiddenError = "Bu amal uchun ruxsat yo'q.";

    public async Task<GroupDto?> GetAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        return group is null || FindMember(group, currentUserId) is null ? null : ToDto(group, currentUserId);
    }

    public async Task<GroupResult> CreateAsync(int currentUserId, CreateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Fail(validation.Errors[0].ErrorMessage);

        var memberIds = request.MemberIds.Where(id => id != currentUserId).Distinct().ToArray();
        if (memberIds.Length == 0 && !request.AutoJoin) return Fail("Kamida bitta a'zo tanlanishi kerak.");

        var users = await repository.FindUsersAsync([currentUserId, .. memberIds], cancellationToken);
        var creator = users.FirstOrDefault(user => user.Id == currentUserId);
        var members = users.Where(user => user.Id != currentUserId).ToArray();
        if (creator is null) return Fail("Foydalanuvchi aniqlanmadi.");
        if (members.Length != memberIds.Length) return Fail("Ba'zi foydalanuvchilar topilmadi.");

        Organization? organization = null;
        if (request.OrganizationOnly)
        {
            var membership = creator.OrganizationMembership;
            if (membership?.Organization is null) return Fail("Tashkilot guruhini faqat tashkilotning tasdiqlangan a'zosi yarata oladi.");
            organization = membership.Organization;
            if (request.AutoJoin && membership.Role != OrganizationRole.Admin)
                return Fail($"Avtomatik qo'shishni faqat {organization.ShortName} admini yoqa oladi.");
            if (members.Any(user => !IsOrganizationMember(user, organization.Id))) return Fail(OrganizationOnlyError(organization));

            if (request.AutoJoin)
            {
                // Everyone already verified joins now; later members join on their first verified sign-in.
                var others = await repository.FindOrganizationUsersAsync(
                    organization.Id, [currentUserId, .. memberIds], GroupLimits.MaxOrganizationMembers - 1 - members.Length, cancellationToken);
                members = [.. members, .. others];
            }
        }

        if (members.Length + 1 > GroupLimits.MaxMembersFor(organization is not null))
            return Fail($"Guruhda {GroupLimits.MaxMembersFor(organization is not null)} tadan ortiq a'zo bo'lishi mumkin emas.");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var group = GroupFactory.Create(request.Title.Trim(), NormalizeDescription(request.Description), creator, members, now, organization, request.AutoJoin);
        await repository.AddGroupAsync(group, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await PublishServiceMessageAsync(group.Chat.Messages.Single(), group, cancellationToken);
        return new(ToDto(group, currentUserId), null);
    }

    public async Task<GroupResult> UpdateAsync(int currentUserId, int chatId, UpdateGroupRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Fail(validation.Errors[0].ErrorMessage);

        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        var actor = group is null ? null : FindMember(group, currentUserId);
        if (group is null || actor is null) return Fail(NotFoundError);
        if (!CanManageGroup(actor)) return Fail(ForbiddenError);

        var title = request.Title.Trim();
        var titleChanged = title != group.Title;
        group.Title = title;
        group.Description = NormalizeDescription(request.Description);
        var serviceMessage = titleChanged
            ? AddServiceMessage(group, actor.User, MessageServiceAction.TitleChanged, title)
            : null;
        await repository.SaveChangesAsync(cancellationToken);

        if (serviceMessage is not null) await PublishServiceMessageAsync(serviceMessage, group, cancellationToken);
        await PublishGroupUpdatedAsync(group, cancellationToken);
        return new(ToDto(group, currentUserId), null);
    }

    public async Task<GroupResult> UpdatePhotoAsync(int currentUserId, int chatId, StoreImageRequest request, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        var actor = group is null ? null : FindMember(group, currentUserId);
        if (group is null || actor is null) return Fail(NotFoundError);
        if (!CanManageGroup(actor)) return Fail(ForbiddenError);

        var storedResult = await fileService.StoreImageAsync(request, cancellationToken);
        if (storedResult.File is null) return Fail(storedResult.Error ?? "Rasm yuklanmadi.");
        var stored = storedResult.File;

        var oldFile = group.Photo?.File;
        var file = new Domain.Entities.File { Name = stored.FileName, MimeType = stored.MimeType, SizeBytes = stored.SizeBytes, StoragePath = stored.StoragePath };
        var photo = new Photo { File = file, Width = stored.Width, Height = stored.Height };
        await repository.AddPhotoAsync(file, photo, cancellationToken);
        group.Photo = photo;
        var serviceMessage = AddServiceMessage(group, actor.User, MessageServiceAction.PhotoChanged, null);
        await repository.SaveChangesAsync(cancellationToken);

        if (oldFile is not null) await fileService.DeleteAsync(oldFile.StoragePath, cancellationToken);

        await PublishServiceMessageAsync(serviceMessage, group, cancellationToken);
        await PublishGroupUpdatedAsync(group, cancellationToken);
        return new(ToDto(group, currentUserId), null);
    }

    public async Task<GroupResult> AddMembersAsync(int currentUserId, int chatId, AddGroupMembersRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await addMembersValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Fail(validation.Errors[0].ErrorMessage);

        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        var actor = group is null ? null : FindMember(group, currentUserId);
        if (group is null || actor is null) return Fail(NotFoundError);
        if (!CanManageGroup(actor)) return Fail(ForbiddenError);

        var existingIds = group.Chat.Members.Select(member => member.UserId).ToHashSet();
        var newIds = request.UserIds.Where(id => !existingIds.Contains(id)).Distinct().ToArray();
        if (newIds.Length == 0) return Fail("Tanlangan foydalanuvchilar allaqachon guruhda.");
        var maxMembers = GroupLimits.MaxMembersFor(group.OrganizationId is not null);
        if (existingIds.Count + newIds.Length > maxMembers) return Fail($"Guruhda {maxMembers} tadan ortiq a'zo bo'lishi mumkin emas.");

        var users = await repository.FindUsersAsync(newIds, cancellationToken);
        if (users.Count != newIds.Length) return Fail("Ba'zi foydalanuvchilar topilmadi.");
        if (group.Organization is { } organization && users.Any(user => !IsOrganizationMember(user, organization.Id)))
            return Fail(OrganizationOnlyError(organization));

        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var user in users)
        {
            group.Chat.Members.Add(GroupFactory.CreateMember(user, ChatMemberRole.Member, now));
        }

        var names = string.Join(", ", users.Select(GroupMapper.DisplayName));
        var serviceMessage = AddServiceMessage(group, actor.User, MessageServiceAction.MembersAdded, names);
        await repository.SaveChangesAsync(cancellationToken);

        await PublishServiceMessageAsync(serviceMessage, group, cancellationToken);
        await PublishGroupUpdatedAsync(group, cancellationToken);
        return new(ToDto(group, currentUserId), null);
    }

    public async Task<GroupResult> RemoveMemberAsync(int currentUserId, int chatId, int userId, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        var actor = group is null ? null : FindMember(group, currentUserId);
        if (group is null || actor is null) return Fail(NotFoundError);

        var target = FindMember(group, userId);
        if (target is null) return Fail("Foydalanuvchi guruh a'zosi emas.");
        if (target.UserId == currentUserId) return Fail("Guruhdan chiqish uchun \"Guruhdan chiqish\" amalidan foydalaning.");
        if (!CanRemove(actor, target)) return Fail(ForbiddenError);

        RemoveMember(group, target);
        var serviceMessage = AddServiceMessage(group, actor.User, MessageServiceAction.MemberRemoved, GroupMapper.DisplayName(target.User));
        await repository.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.ChatDeletedAsync(chatId, [target.UserId], cancellationToken);
        await PublishServiceMessageAsync(serviceMessage, group, cancellationToken);
        await PublishGroupUpdatedAsync(group, cancellationToken);
        return new(ToDto(group, currentUserId), null);
    }

    public async Task<GroupResult> UpdateMemberRoleAsync(int currentUserId, int chatId, int userId, UpdateGroupMemberRoleRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await updateRoleValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return Fail(validation.Errors[0].ErrorMessage);

        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        var actor = group is null ? null : FindMember(group, currentUserId);
        if (group is null || actor is null) return Fail(NotFoundError);
        if (actor.Role != ChatMemberRole.Creator) return Fail("Faqat guruh egasi adminlarni tayinlay oladi.");

        var target = FindMember(group, userId);
        if (target is null) return Fail("Foydalanuvchi guruh a'zosi emas.");
        if (target.Role == ChatMemberRole.Creator) return Fail("Guruh egasining rolini o'zgartirib bo'lmaydi.");

        target.Role = request.Role;
        await repository.SaveChangesAsync(cancellationToken);

        await PublishGroupUpdatedAsync(group, cancellationToken);
        return new(ToDto(group, currentUserId), null);
    }

    public async Task<GroupLeaveResult> LeaveAsync(int currentUserId, int chatId, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        var leaver = group is null ? null : FindMember(group, currentUserId);
        if (group is null || leaver is null) return new(false, NotFoundError);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        RemoveMember(group, leaver);

        if (group.Chat.Members.Count == 0)
        {
            group.Chat.DeletedAt = now;
            await repository.SaveChangesAsync(cancellationToken);
            await realtimeNotifier.ChatDeletedAsync(chatId, [currentUserId], cancellationToken);
            return new(true, null);
        }

        if (leaver.Role == ChatMemberRole.Creator)
        {
            // Ownership passes to the longest-serving admin, or to the longest-serving member if there are no admins.
            var successor = group.Chat.Members
                .OrderByDescending(member => member.Role == ChatMemberRole.Admin)
                .ThenBy(member => member.JoinedAt)
                .ThenBy(member => member.Id)
                .First();
            successor.Role = ChatMemberRole.Creator;
            group.CreatorId = successor.UserId;
            group.Creator = successor.User;
        }

        var serviceMessage = AddServiceMessage(group, leaver.User, MessageServiceAction.MemberLeft, GroupMapper.DisplayName(leaver.User));
        await repository.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.ChatDeletedAsync(chatId, [currentUserId], cancellationToken);
        await PublishServiceMessageAsync(serviceMessage, group, cancellationToken);
        await PublishGroupUpdatedAsync(group, cancellationToken);
        return new(true, null);
    }

    public async Task<bool> CreateOrganizationGroupAsync(int userId, CancellationToken cancellationToken = default)
    {
        var owner = (await repository.FindUsersAsync([userId], cancellationToken)).FirstOrDefault();
        var organization = owner?.OrganizationMembership?.Organization;
        if (owner is null || organization is null) return false;

        var members = await repository.FindOrganizationUsersAsync(
            organization.Id, [userId], GroupLimits.MaxOrganizationMembers - 1, cancellationToken);
        var group = GroupFactory.Create(
            $"{organization.ShortName} jamoasi",
            $"@{organization.Domain} pochtasi bilan kirgan hamma shu yerda. Guruh avtomatik yaratilgan.",
            owner, members, timeProvider.GetUtcNow().UtcDateTime, organization, autoJoin: true);
        await repository.AddGroupAsync(group, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        await PublishServiceMessageAsync(group.Chat.Messages.Single(), group, cancellationToken);
        return true;
    }

    public async Task<bool> JoinViaOrganizationAsync(int chatId, int userId, CancellationToken cancellationToken = default)
    {
        var group = await repository.GetGroupAsync(chatId, cancellationToken);
        if (group?.Organization is null || FindMember(group, userId) is not null) return false;
        if (group.Chat.Members.Count >= GroupLimits.MaxOrganizationMembers) return false;

        var user = (await repository.FindUsersAsync([userId], cancellationToken)).FirstOrDefault();
        if (user is null || !IsOrganizationMember(user, group.Organization.Id)) return false;

        group.Chat.Members.Add(GroupFactory.CreateMember(user, ChatMemberRole.Member, timeProvider.GetUtcNow().UtcDateTime));
        var serviceMessage = AddServiceMessage(group, user, MessageServiceAction.MemberJoinedViaOrganization, group.Organization.ShortName);
        await repository.SaveChangesAsync(cancellationToken);

        await PublishServiceMessageAsync(serviceMessage, group, cancellationToken);
        await PublishGroupUpdatedAsync(group, cancellationToken);
        return true;
    }

    private static bool IsOrganizationMember(User user, int organizationId) =>
        user.OrganizationMembership?.OrganizationId == organizationId;

    private static string OrganizationOnlyError(Organization organization) =>
        $"Bu guruhga faqat {organization.ShortName} a'zolarini qo'shish mumkin.";

    private static bool CanManageGroup(ChatMember actor) =>
        actor.Role is ChatMemberRole.Admin or ChatMemberRole.Creator;

    private static bool CanRemove(ChatMember actor, ChatMember target) => actor.Role switch
    {
        ChatMemberRole.Creator => true,
        ChatMemberRole.Admin => target.Role == ChatMemberRole.Member,
        _ => false
    };

    private static ChatMember? FindMember(Group group, int userId) =>
        group.Chat.Members.FirstOrDefault(member => member.UserId == userId);

    private void RemoveMember(Group group, ChatMember member)
    {
        group.Chat.Members.Remove(member);
        repository.RemoveMember(member);
    }

    private Message AddServiceMessage(Group group, User actor, MessageServiceAction action, string? textContent)
    {
        var message = MessageFactory.CreateService(group.ChatId, actor.Id, action, textContent, timeProvider.GetUtcNow().UtcDateTime);
        message.Sender = actor;
        group.Chat.Messages.Add(message);
        return message;
    }

    private Task PublishServiceMessageAsync(Message message, Group group, CancellationToken cancellationToken) =>
        realtimeNotifier.MessageCreatedAsync(MessageMapper.ToDto(message), MemberIds(group), cancellationToken);

    private Task PublishGroupUpdatedAsync(Group group, CancellationToken cancellationToken) =>
        realtimeNotifier.GroupUpdatedAsync(group.ChatId, group.Title, group.Description, group.PhotoId, group.Chat.Members.Count, MemberIds(group), cancellationToken);

    private static IReadOnlyCollection<int> MemberIds(Group group) =>
        group.Chat.Members.Select(member => member.UserId).ToArray();

    private GroupDto ToDto(Group group, int currentUserId) =>
        GroupMapper.ToDto(group, currentUserId, presenceTracker.IsOnline);

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static GroupResult Fail(string error) => new(null, error);
}
