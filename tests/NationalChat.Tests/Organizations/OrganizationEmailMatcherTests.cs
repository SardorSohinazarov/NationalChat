using Application.Features.Organizations;

namespace NationalChat.Tests.Organizations;

public sealed class OrganizationEmailMatcherTests
{
    [Theory]
    [InlineData("ali@tuit.uz", "tuit.uz", true)]
    [InlineData("Ali@TUIT.UZ", "tuit.uz", true)]
    [InlineData("ali@tuit.uz.", "tuit.uz", true)]
    [InlineData("ali@tuit.uz", "TUIT.uz.", true)]
    [InlineData("ali@evil-tuit.uz", "tuit.uz", false)]
    [InlineData("ali@tuit.uz.evil.com", "tuit.uz", false)]
    [InlineData("ali@student.tuit.uz", "tuit.uz", false)]
    [InlineData("ali@student.tuit.uz", "student.tuit.uz", true)]
    [InlineData("tuit.uz", "tuit.uz", false)]
    [InlineData("", "tuit.uz", false)]
    public void Matches_OnlyExactDomain(string email, string domain, bool expected)
    {
        Assert.Equal(expected, OrganizationEmailMatcher.Matches(email, domain));
    }

    [Theory]
    [InlineData("gmail.com", true)]
    [InlineData("GMAIL.COM", true)]
    [InlineData("mail.ru", true)]
    [InlineData("yandex.ru", true)]
    [InlineData("tuit.uz", false)]
    [InlineData("gmail.com.uz", false)]
    public void IsPublicEmailDomain(string domain, bool expected)
    {
        Assert.Equal(expected, OrganizationEmailMatcher.IsPublicEmailDomain(domain));
    }

    [Theory]
    [InlineData("  Tuit.UZ. ", "tuit.uz")]
    [InlineData("localhost", null)]
    [InlineData("tuit..uz", null)]
    [InlineData("-tuit.uz", null)]
    [InlineData("tu it.uz", null)]
    [InlineData("tuit.uz/x", null)]
    public void NormalizeDomain(string domain, string? expected)
    {
        Assert.Equal(expected, OrganizationEmailMatcher.NormalizeDomain(domain));
    }
}
