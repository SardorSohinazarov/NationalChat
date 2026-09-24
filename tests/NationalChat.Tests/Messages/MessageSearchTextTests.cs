using Application.Features.Messages;
using Application.Features.Messages.DataTransferObjects.Requests;
using Application.Features.Messages.DataTransferObjects.Responses;
using Application.Features.Messages.Factories;
using Application.Features.Messages.Validators;
using Domain.Entities;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Messages;

public sealed class MessageSearchTextTests
{
    private const int UserId = 1;
    private const int ChatId = 10;

    private readonly IMessageRepository _repository = Substitute.For<IMessageRepository>();
    private readonly MessageService _service;

    public MessageSearchTextTests()
    {
        _repository.IsChatMemberAsync(ChatId, UserId, Arg.Any<CancellationToken>()).Returns(true);
        _service = new MessageService(
            _repository,
            Substitute.For<IChatRealtimeNotifier>(),
            new SendMessageRequestValidator(),
            new UpdateMessageRequestValidator(),
            new MessageSearchRequestValidator(),
            new FixedTimeProvider(new DateTimeOffset(TestData.Start)));
    }

    [Fact]
    public void Create_StoresOriginalTextAndLatinSearchText()
    {
        var message = MessageFactory.Create(ChatId, UserId, "  Салом, Қалайсиз?  ", null, TestData.Start);

        Assert.Equal("Салом, Қалайсиз?", message.TextContent);
        Assert.Equal("salom, qalaysiz?", message.SearchText);
    }

    [Fact]
    public void Create_WithoutText_HasNoSearchText()
    {
        Assert.Null(MessageFactory.Create(ChatId, UserId, "   ", null, TestData.Start).SearchText);
    }

    [Theory]
    [InlineData("o'zbek")]
    [InlineData("o‘zbek")]
    [InlineData("oʻzbek")]
    [InlineData("ЎЗБЕК")]
    public async Task Search_PassesScriptIndependentQueryToRepository(string query)
    {
        _repository.SearchAsync(ChatId, UserId, Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MessageDto>());

        await _service.SearchAsync(UserId, ChatId, new MessageSearchRequest(query));

        await _repository.Received(1).SearchAsync(ChatId, UserId, "o'zbek", 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_RefreshesSearchText()
    {
        var message = MessageFactory.Create(ChatId, UserId, "salom", null, TestData.Start);
        message.Id = 5;
        _repository.GetOwnedMessageAsync(ChatId, 5, UserId, Arg.Any<CancellationToken>()).Returns(message);

        await _service.UpdateAsync(UserId, ChatId, 5, new UpdateMessageRequest("Хайр, дўстим"));

        Assert.Equal("Хайр, дўстим", message.TextContent);
        Assert.Equal("xayr, do'stim", message.SearchText);
    }
}
