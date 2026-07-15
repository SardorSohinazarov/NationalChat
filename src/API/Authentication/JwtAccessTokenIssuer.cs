using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace API.Authentication;

public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;

    public JwtAccessTokenIssuer(JwtOptions options)
    {
        _options = options;
        _credentials = new SigningCredentials(new SymmetricSecurityKey(options.GetSigningKey()), SecurityAlgorithms.HmacSha256);
    }

    public string Create(int userId, int sessionId, DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("sid", sessionId.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: _credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
