using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace E_Commerce.Api.Extensions;

public static class AuthenticationExtensions
{
    public static void AddAuthenticationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration.GetValue<string>("ApiSettings:Secret");
        var key = Encoding.ASCII.GetBytes(secret ?? "DefaultSecretKeyForDevelopmentOnly");

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });
    }
}
