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
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using API.DataTransferObjects.Responses;

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

        services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
        services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
        {
            var message = string.Join(" ", context.ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage)
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            return new BadRequestObjectResult(Result.Fail(
                string.IsNullOrWhiteSpace(message) ? "So'rov ma'lumotlari noto'g'ri." : message));
        });
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
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(Result.Fail("Autentifikatsiya talab qilinadi."));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(Result.Fail("Bu amal uchun ruxsat yo'q."));
                    }
                };
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
