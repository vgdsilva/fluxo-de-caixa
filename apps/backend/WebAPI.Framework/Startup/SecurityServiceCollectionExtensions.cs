using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Common.Authentication;
using WebAPI.Common.Configuration;
using WebAPI.Framework.Security;

namespace WebAPI.Framework.Startup;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddWebAPIJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtOptions.SectionName);
        services.Configure<JwtOptions>(section);
        var jwt = section.Get<JwtOptions>() ?? new JwtOptions();

        if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "Configure Jwt:SigningKey com pelo menos 32 caracteres (use dotnet user-secrets em desenvolvimento).");
        }

        services.TryAddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                };
            });

        services.AddAuthorization();
        return services;
    }
}
