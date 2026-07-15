using Application.Features.Authentication;
using Application.Features.Contacts;
using Application.Features.Profiles;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security.Hashing;
using Infrastructure.Security.Jwt;
using Infrastructure.Security.Options;
using Infrastructure.Security.Tokens;
using Infrastructure.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var authOptions = configuration.GetSection("Auth").Get<AuthOptions>() ?? new AuthOptions();
        var authSecurityOptions = configuration.GetSection("AuthSecurity").Get<AuthSecurityOptions>() ?? new AuthSecurityOptions();
        var smtpOptions = configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
        var signingKey = jwtOptions.GetSigningKey();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.AddDbContext<ChatDb>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            });
        services.AddAuthorization();

        services.AddSingleton(jwtOptions);
        services.AddSingleton(authOptions);
        services.AddSingleton(authSecurityOptions);
        services.AddSingleton(smtpOptions);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddSingleton<IOneTimeCodeHasher, Pbkdf2OneTimeCodeHasher>();
        services.AddSingleton<IRefreshTokenHasher, HmacRefreshTokenHasher>();
        services.AddSingleton<IRegistrationTokenService, HmacRegistrationTokenService>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();

        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(smtpOptions.Host))
        {
            services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
        }
        else
        {
            if (string.IsNullOrWhiteSpace(smtpOptions.Host) || string.IsNullOrWhiteSpace(smtpOptions.FromAddress))
            {
                throw new InvalidOperationException("Smtp:Host va Smtp:FromAddress sozlanishi kerak.");
            }

            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }

        return services;
    }
}
