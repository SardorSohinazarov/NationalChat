using API.Options;

namespace NationalChat.Tests.Security;

public sealed class ClientOriginOptionsTests
{
    private static readonly ClientOriginOptions Options = new()
    {
        AllowedOrigins = ["https://milliychat.uz/", "https://national-chat-client-*-sardors-projects-56e94522.vercel.app"],
    };

    [Theory]
    [InlineData("https://milliychat.uz")]
    [InlineData("https://MILLIYCHAT.uz")]
    [InlineData("https://national-chat-client-git-main-sardors-projects-56e94522.vercel.app")]
    [InlineData("https://national-chat-client-abc123-sardors-projects-56e94522.vercel.app")]
    public void AllowsConfiguredOrigins(string origin) => Assert.True(Options.IsAllowed(origin));

    [Theory]
    [InlineData("http://milliychat.uz")]
    [InlineData("https://milliychat.uz.evil.com")]
    [InlineData("https://evil-milliychat.uz")]
    [InlineData("https://milliychat.uz:8443")]
    // The wildcard never crosses a dot, so it cannot pull in another domain.
    [InlineData("https://national-chat-client-x.evil.com-sardors-projects-56e94522.vercel.app")]
    [InlineData("https://national-chat-client-x-sardors-projects-56e94522.vercel.app.evil.com")]
    [InlineData("https://someone-else.vercel.app")]
    [InlineData("null")]
    public void RejectsEverythingElse(string origin) => Assert.False(Options.IsAllowed(origin));

    [Fact]
    public void EmptyOrBlankConfiguration_IsNotRestricted()
    {
        Assert.False(new ClientOriginOptions().IsRestricted);
        Assert.False(new ClientOriginOptions { AllowedOrigins = ["", "  "] }.IsRestricted);
        Assert.True(Options.IsRestricted);
    }
}
