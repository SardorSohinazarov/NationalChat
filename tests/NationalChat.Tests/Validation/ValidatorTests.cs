using Application.DataTransferObjects.Pagination;
using Application.Features.Groups;
using Application.Features.Groups.DataTransferObjects.Requests;
using Application.Features.Groups.Validators;
using Domain.Entities;

namespace NationalChat.Tests.Validation;

public sealed class ValidatorTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void CursorPagination_LimitMustBeBetween1And100(int limit, bool expected)
    {
        var result = new CursorPaginationRequestValidator().Validate(new CursorPaginationRequest { Limit = limit });

        Assert.Equal(expected, result.IsValid);
    }

    [Fact]
    public void CreateGroup_TitleLongerThanLimit_IsInvalid()
    {
        var request = new CreateGroupRequest(new string('a', GroupLimits.TitleMaxLength + 1), null, [2]);

        Assert.False(new CreateGroupRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void CreateGroup_WithoutMembers_IsInvalid()
    {
        Assert.False(new CreateGroupRequestValidator().Validate(new CreateGroupRequest("Guruh", null, [])).IsValid);
    }

    [Fact]
    public void AddMembers_NonPositiveId_IsInvalid()
    {
        Assert.False(new AddGroupMembersRequestValidator().Validate(new AddGroupMembersRequest([0])).IsValid);
    }

    [Theory]
    [InlineData(ChatMemberRole.Member, true)]
    [InlineData(ChatMemberRole.Admin, true)]
    [InlineData(ChatMemberRole.Creator, false)]
    public void UpdateRole_OnlyMemberOrAdmin(ChatMemberRole role, bool expected)
    {
        Assert.Equal(expected, new UpdateGroupMemberRoleRequestValidator().Validate(new UpdateGroupMemberRoleRequest(role)).IsValid);
    }
}
