using Infrastructure.Security.Hashing;
using Infrastructure.Security.Options;
using Infrastructure.Security.Tokens;

namespace NationalChat.Tests.Security;

public sealed class Pbkdf2OneTimeCodeHasherTests
{
    private readonly Pbkdf2OneTimeCodeHasher _hasher = new();

    [Fact]
    public void Verify_CorrectCode_ReturnsTrue()
    {
        var hash = _hasher.Hash("482913");

        Assert.True(_hasher.Verify("482913", hash));
    }

    [Fact]
    public void Verify_WrongCode_ReturnsFalse()
    {
        var hash = _hasher.Hash("482913");

        Assert.False(_hasher.Verify("482914", hash));
    }

    [Fact]
    public void Hash_SameCodeTwice_UsesDifferentSaltAndIterationPrefix()
    {
        var first = _hasher.Hash("482913");
        var second = _hasher.Hash("482913");

        Assert.NotEqual(first, second);
        Assert.StartsWith("210000.", first);
        Assert.DoesNotContain("482913", first);
    }

    [Fact]
    public void Verify_MalformedHash_ReturnsFalse()
    {
        Assert.False(_hasher.Verify("482913", "not-a-hash"));
    }
}

public sealed class HmacRefreshTokenHasherTests
{
    private static AuthSecurityOptions Options(string secret) =>
        new() { RefreshTokenHashKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(secret)) };

    [Fact]
    public void Hash_IsDeterministicForSameKey()
    {
        var hasher = new HmacRefreshTokenHasher(Options("test-refresh-hash-key-0123456789abcdef"));

        Assert.Equal(hasher.Hash("token-value"), hasher.Hash("token-value"));
    }

    [Fact]
    public void Hash_DependsOnKey()
    {
        var first = new HmacRefreshTokenHasher(Options("test-refresh-hash-key-0123456789abcdef"));
        var second = new HmacRefreshTokenHasher(Options("another-refresh-hash-key-0123456789ab"));

        Assert.NotEqual(first.Hash("token-value"), second.Hash("token-value"));
    }

    [Fact]
    public void Constructor_ShortKey_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new HmacRefreshTokenHasher(Options("short")));
    }
}
