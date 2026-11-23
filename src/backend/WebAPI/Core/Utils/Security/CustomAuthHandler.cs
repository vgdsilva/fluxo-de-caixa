using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace WebAPI.Core.Utils.Security;

/// <summary>Autentica a requisição pelo JWT de acesso no header <c>Authorization: Bearer</c>.</summary>
public class CustomAuthHandler(IOptionsMonitor<CustomAuthOptions> options,
                               ILoggerFactory logger,
                               UrlEncoder encoder,
                               TokenManager tokenManager)
    : AuthenticationHandler<CustomAuthOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out AuthenticationHeaderValue? authHeader))
            return AuthenticateResult.NoResult();

        if (!authHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(authHeader.Parameter))
            return AuthenticateResult.Fail("Authorization Header inválido");

        ClaimsIdentity? identity = await tokenManager.ValidateJwtTokenAsync(authHeader.Parameter, Scheme.Name);

        if (identity is null)
            return AuthenticateResult.Fail("Authorization Token inválido ou expirado");

        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }
}
