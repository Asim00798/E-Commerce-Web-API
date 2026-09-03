using E_Commerce.Application.BoundedContexts.Onboarding.Abstractions;
using E_Commerce.Application.Modules.Authentication.Abstractions;
using E_Commerce.Application.Modules.Authentication.Constants;
using E_Commerce.Application.Modules.Authorization.Abstractions;
using E_Commerce.Application.Modules.Identity.AccountManagement.Abstractions;
using E_Commerce.Application.Shared.Security.Authorization.Services;
using E_Commerce.Application.Shared.Security.Cryptography;
using E_Commerce.Application.Shared.Security.Identity;
using E_Commerce.Application.Shared.Security.Verification;
using E_Commerce.Domain.BoundedContexts.UserManagement.Onboarding.Repositories;
using E_Commerce.Infrastructure.Identity.Services;
using E_Commerce.Infrastructure.Persistence.Modules.Onboarding.Repositories;
using E_Commerce.Infrastructure.Persistence.Modules.Onboarding.Services;
using E_Commerce.Infrastructure.Persistence.Modules.Security.Authentication.Repositories;
using E_Commerce.Infrastructure.Persistence.Modules.Security.Authorization.Repositories;
using E_Commerce.Infrastructure.Security.Authentication.Services;
using E_Commerce.Infrastructure.Security.Authentication.Tokens.Jwt;
using E_Commerce.Infrastructure.Security.Authentication.Tokens.Refresh;
using E_Commerce.Infrastructure.Security.Authorization.Policies;
using E_Commerce.Infrastructure.Security.Authorization.Services;
using E_Commerce.Infrastructure.Security.Cryptography;
using E_Commerce.Infrastructure.Security.Identity.Services;
using E_Commerce.Infrastructure.Security.Verification;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace E_Commerce.Infrastructure.Security.Extensions;

/// <summary>
/// Extension methods for registering all security‑related infrastructure services.
/// </summary>
public static class SecurityInfrastructureExtensions
{
    /// <summary>
    /// Registers identity, onboarding, authentication, and token services.
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddVerificationAndPasswordHashing(services, configuration);
        AddRegistrationServices(services);
        AddIdentityServices(services);
        AddAccountManagementServices(services);
        AddAuthenticationServices(services, configuration);
        ConfigureAuthenticationSchemes(services, configuration);

        return services;
    }

    // ---------------------------------------------------------------
    // Verification & password hashing
    // ---------------------------------------------------------------

    private static void AddVerificationAndPasswordHashing(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<VerificationOptions>(
            configuration.GetSection(VerificationOptions.SectionName));

        services.AddScoped<IVerificationCodeService, VerificationCodeService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }

    // ---------------------------------------------------------------
    // Registration & onboarding services
    // ---------------------------------------------------------------

    private static void AddRegistrationServices(
        IServiceCollection services)
    {
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IRegistrationCleanupService, RegistrationCleanupService>();
    }

    // ---------------------------------------------------------------
    // Identity / user management services
    // ---------------------------------------------------------------

    private static void AddIdentityServices(
        IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
    }

    // ---------------------------------------------------------------
    // Account management, credentials, and queries
    // ---------------------------------------------------------------

    private static void AddAccountManagementServices(
        IServiceCollection services)
    {
        services.AddScoped<IAccountManagement, AccountManagementService>();
        services.AddScoped<ICredentialManagement, CredentialManagementService>();
        services.AddScoped<IAccountReader, AccountReader>();
    }

    // ---------------------------------------------------------------
    // Authentication & token services
    // ---------------------------------------------------------------

    private static void AddAuthenticationServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        ConfigureRefreshTokenHasherOptions(services, configuration);
        ConfigureJwtOptions(services, configuration);

        services.AddDataProtection();

        services.AddScoped<JwtTokenGenerator>();
        services.AddScoped<RefreshTokenHasher>();
        services.AddScoped<RefreshTokenRepository>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddScoped<IUserLinkStateProtector, UserLinkStateProtector>();
        services.AddOptions<RefreshTokenOptions>()
                .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
                .Validate(x => x.TokenLifetime > TimeSpan.Zero,
                    "Refresh token lifetime must be greater than zero.")
                .ValidateOnStart();
    }

    private static void ConfigureJwtOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.Issuer),
                "JWT issuer is required.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.Audience),
                "JWT audience is required.")
            .Validate(x => !string.IsNullOrWhiteSpace(x.Secret),
                "JWT secret is required.")
            .Validate(x =>
                    System.Text.Encoding.UTF8.GetByteCount(x.Secret) >= 32,
                "JWT secret must be at least 32 bytes.")
            .Validate(x => x.ExpiryMinutes > 0,
                "JWT expiry must be greater than zero.")
            .ValidateOnStart();
    }

    private static void ConfigureRefreshTokenHasherOptions(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RefreshTokenHasherOptions>()
            .Bind(configuration.GetSection(RefreshTokenHasherOptions.SectionName))
            .Validate(x => !string.IsNullOrWhiteSpace(x.SecretKey),
                "Refresh token hashing secret is required.")
            .Validate(x =>
            {
                try
                {
                    return Convert.FromBase64String(x.SecretKey).Length >= 32;
                }
                catch
                {
                    return false;
                }
            },
            "Refresh token hashing secret must be valid Base64 and at least 256 bits.")
            .ValidateOnStart();
    }

    // ---------------------------------------------------------------
    // Authentication schemes
    // ---------------------------------------------------------------

    private static void ConfigureAuthenticationSchemes(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = AuthenticationConstants.ExternalCookieScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var jwtSettings = configuration
                .GetSection(JwtSettings.SectionName)
                .Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "JWT configuration is missing.");

            options.TokenValidationParameters =
                JwtAuthenticationConfiguration.Create(jwtSettings);
        })
        .AddCookie(AuthenticationConstants.ExternalCookieScheme, options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            options.SlidingExpiration = false;
            options.Cookie.Name = "__Host-ECommerce.External";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.LoginPath = "/api/auth/login";
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"]
                ?? throw new InvalidOperationException(
                    "Google ClientId is missing.");

            options.ClientSecret = configuration["Authentication:Google:ClientSecret"]
                ?? throw new InvalidOperationException(
                    "Google ClientSecret is missing.");

            options.SignInScheme = AuthenticationConstants.ExternalCookieScheme;
            options.Scope.Add("email");
            options.SaveTokens = false;
        });

        // ---------------------------------------------------------------
        // Authorization
        // ---------------------------------------------------------------

        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserRoleService, UserRoleService>();

        services.AddScoped<PermissionRepository>();
        services.AddScoped<RolePermissionRepository>();

        // Optional management services
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IPermissionManagementService, PermissionManagementService>();

        // Authorization policy provider and handler
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }
}