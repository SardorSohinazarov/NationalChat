using API.DataTransferObjects.Responses;
using Application.Features.Authentication;
using Application.Features.Authentication.Validators;
using Application.Features.Contacts;
using Application.Features.Profiles;
using Application.Features.Chats;
using Application.Features.Users;
using Application.Features.Messages;
using Infrastructure.Email;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security.Hashing;
using Infrastructure.Security.Jwt;
using Infrastructure.Security.Options;
using Infrastructure.Security.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

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
        services
            .AddApiTransport()
            .AddApiDocumentation()
            .AddPersistence(configuration)
            .AddJwtAuthentication(jwtOptions)
            .AddApplicationServices()
            .AddSecurityServices(jwtOptions, authOptions, authSecurityOptions)
            .AddEmailServices(smtpOptions, environment);

        return services;
    }

    private static IServiceCollection AddApiTransport(this IServiceCollection services)
    {
        services.AddCors(options => options.AddPolicy("Client", policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
        services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);
        services.AddFluentValidationAutoValidation();
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
        return services;
    }

    private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
            });
        });
        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChatDb>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IProfileRepository, ProfileRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IUserDiscoveryRepository, UserDiscoveryRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        return services;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtOptions jwtOptions)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true, ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(jwtOptions.GetSigningKey()),
                ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1)
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
        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RequestSignInCodeCommandValidator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IUserDiscoveryService, UserDiscoveryService>();
        services.AddScoped<IMessageService, MessageService>();
        return services;
    }

    private static IServiceCollection AddSecurityServices(this IServiceCollection services, JwtOptions jwtOptions, AuthOptions authOptions, AuthSecurityOptions authSecurityOptions)
    {
        services.AddSingleton(jwtOptions);
        services.AddSingleton(authOptions);
        services.AddSingleton(authSecurityOptions);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOneTimeCodeHasher, Pbkdf2OneTimeCodeHasher>();
        services.AddSingleton<IRefreshTokenHasher, HmacRefreshTokenHasher>();
        services.AddSingleton<IRegistrationTokenService, HmacRegistrationTokenService>();
        services.AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>();
        return services;
    }

    private static IServiceCollection AddEmailServices(this IServiceCollection services, SmtpOptions smtpOptions, IWebHostEnvironment environment)
    {
        services.AddSingleton(smtpOptions);
        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(smtpOptions.Host))
        {
            services.AddSingleton<IEmailSender, DevelopmentEmailSender>();
            return services;
        }

        if (string.IsNullOrWhiteSpace(smtpOptions.Host) || string.IsNullOrWhiteSpace(smtpOptions.FromAddress))
        {
            throw new InvalidOperationException("Smtp:Host va Smtp:FromAddress sozlanishi kerak.");
        }

        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
