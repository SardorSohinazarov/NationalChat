using Application.Features.Authentication;
using Application.Features.Authentication.DataTransferObjects.Commands;
using Application.Features.Organizations;
using Application.Features.SecretChats;
using Domain.Entities;
using FluentValidation;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Authentication;

/// <summary>A device that is logged out or revoked must lose its secret chats immediately.</summary>
public sealed class AuthServiceSecretChatTests
{
    private const int UserId = 1;

    private readonly IAuthRepository _store = Substitute.For<IAuthRepository>();
    private readonly ISecretChatService _secretChats = Substitute.For<ISecretChatService>();
    private readonly AuthService _service;
    private readonly List<Session> _sessions = [Session(11), Session(12), Session(13)];

    public AuthServiceSecretChatTests()
    {
        _store.FindSessionAsync(UserId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(call => _sessions.FirstOrDefault(s => s.Id == call.ArgAt<int>(1)));
        _store.GetActiveSessionsAsync(UserId, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(_ => _sessions.Where(s => s.RevokedAt == null).ToList());

        _service = new AuthService(
            _store,
            Substitute.For<IOneTimeCodeHasher>(),
            Substitute.For<IRefreshTokenHasher>(),
            Substitute.For<IAccessTokenIssuer>(),
            Substitute.For<IRegistrationTokenService>(),
            Substitute.For<IGoogleTokenValidator>(),
            Substitute.For<IEmailSender>(),
            Substitute.For<IOrganizationMembershipService>(),
            _secretChats,
            new FixedTimeProvider(new DateTimeOffset(TestData.Start)),
            new AuthOptions(),
            Substitute.For<IValidator<RequestSignInCodeCommand>>(),
            Substitute.For<IValidator<VerifySignInCodeCommand>>(),
            Substitute.For<IValidator<GoogleSignInCommand>>(),
            Substitute.For<IValidator<CompleteRegistrationCommand>>(),
            Substitute.For<IValidator<RefreshSessionCommand>>());
    }

    private static Session Session(int id) => new() { Id = id, UserId = UserId, ExpiresAt = TestData.Start.AddDays(30) };

    private static IReadOnlyCollection<int> Ids(params int[] ids) =>
        Arg.Is<IReadOnlyCollection<int>>(actual => actual.Order().SequenceEqual(ids));

    [Fact]
    public async Task Logout_ClosesSecretChatsOfThisDevice()
    {
        await _service.LogoutAsync(UserId, 12);

        await _secretChats.Received(1).CloseForSessionsAsync(Ids(12), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeSession_ClosesSecretChatsOfTheRevokedDevice()
    {
        Assert.True(await _service.RevokeSessionAsync(UserId, 13, currentSessionId: 11));

        await _secretChats.Received(1).CloseForSessionsAsync(Ids(13), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAllOthers_ClosesSecretChatsOfEveryOtherDevice()
    {
        await _service.LogoutAllOthersAsync(UserId, currentSessionId: 11);

        await _secretChats.Received(1).CloseForSessionsAsync(Ids(12, 13), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogoutAll_ClosesSecretChatsOfEveryDevice()
    {
        await _service.LogoutAllAsync(UserId);

        await _secretChats.Received(1).CloseForSessionsAsync(Ids(11, 12, 13), Arg.Any<CancellationToken>());
    }
}
