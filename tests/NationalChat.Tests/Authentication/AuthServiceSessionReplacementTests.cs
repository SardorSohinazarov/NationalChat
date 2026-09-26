using Application.Features.Authentication;
using Application.Features.Authentication.DataTransferObjects.Commands;
using Application.Features.Authentication.DataTransferObjects.Session;
using Application.Features.Organizations;
using Application.Features.SecretChats;
using Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using NationalChat.Tests.Support;
using NSubstitute;

namespace NationalChat.Tests.Authentication;

/// <summary>Signing in again from the same browser replaces its session instead of adding another device.</summary>
public sealed class AuthServiceSessionReplacementTests
{
    private readonly IAuthRepository _store = Substitute.For<IAuthRepository>();
    private readonly ISecretChatService _secretChats = Substitute.For<ISecretChatService>();
    private readonly List<Session> _added = [];
    private readonly Session _previous = new() { Id = 7, UserId = 1, ExpiresAt = TestData.Start.AddDays(10) };
    private readonly AuthService _service;

    public AuthServiceSessionReplacementTests()
    {
        var user = TestData.User(1, "Ali");
        _store.FindUserByEmailAsync("ali@tuit.uz", Arg.Any<CancellationToken>()).Returns(user);
        _store.FindSessionByRefreshTokenHashAsync("hash:old-token", Arg.Any<CancellationToken>()).Returns(_previous);
        _store.AddSessionAsync(Arg.Do<Session>(_added.Add), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var hasher = Substitute.For<IRefreshTokenHasher>();
        hasher.Hash(Arg.Any<string>()).Returns(call => "hash:" + call.Arg<string>());
        var google = Substitute.For<IGoogleTokenValidator>();
        google.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new GoogleIdentity("ali@tuit.uz", true));
        var googleValidator = Substitute.For<IValidator<GoogleSignInCommand>>();
        googleValidator.ValidateAsync(Arg.Any<GoogleSignInCommand>(), Arg.Any<CancellationToken>()).Returns(new ValidationResult());

        _service = new AuthService(
            _store,
            Substitute.For<IOneTimeCodeHasher>(),
            hasher,
            Substitute.For<IAccessTokenIssuer>(),
            Substitute.For<IRegistrationTokenService>(),
            google,
            Substitute.For<IEmailSender>(),
            Substitute.For<IOrganizationMembershipService>(),
            _secretChats,
            new FixedTimeProvider(new DateTimeOffset(TestData.Start)),
            new AuthOptions(),
            Substitute.For<IValidator<RequestSignInCodeCommand>>(),
            Substitute.For<IValidator<VerifySignInCodeCommand>>(),
            googleValidator,
            Substitute.For<IValidator<CompleteRegistrationCommand>>(),
            Substitute.For<IValidator<RefreshSessionCommand>>());
    }

    private static AuthSessionMetadata Browser(string? previousRefreshToken) =>
        new("Chrome", "Windows", "Web", "::1", "Mozilla/5.0") { ReplacedRefreshToken = previousRefreshToken };

    [Fact]
    public async Task SigningInAgain_RevokesThisBrowsersPreviousSession_AndClosesItsSecretChats()
    {
        var result = await _service.SignInWithGoogleAsync(new GoogleSignInCommand("id-token", Browser("old-token")));

        Assert.True(result.IsSuccessful);
        Assert.Equal(TestData.Start, _previous.RevokedAt);
        Assert.Single(_added);
        await _secretChats.Received(1).CloseForSessionsAsync(
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 7 })), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FirstSignInInThisBrowser_KeepsOtherSessions()
    {
        await _service.SignInWithGoogleAsync(new GoogleSignInCommand("id-token", Browser(null)));

        Assert.Null(_previous.RevokedAt);
        Assert.Single(_added);
        await _secretChats.DidNotReceive().CloseForSessionsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnknownOrAlreadyEndedPreviousSession_IsIgnored()
    {
        _previous.RevokedAt = TestData.Start.AddDays(-1);

        await _service.SignInWithGoogleAsync(new GoogleSignInCommand("id-token", Browser("old-token")));
        await _service.SignInWithGoogleAsync(new GoogleSignInCommand("id-token", Browser("never-issued")));

        Assert.Equal(TestData.Start.AddDays(-1), _previous.RevokedAt);
        Assert.Equal(2, _added.Count);
        await _secretChats.DidNotReceive().CloseForSessionsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void RefreshToken_NeverAppearsInToString()
    {
        Assert.DoesNotContain("old-token", Browser("old-token").ToString());
    }
}
