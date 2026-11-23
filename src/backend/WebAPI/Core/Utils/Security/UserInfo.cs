using System.Security.Claims;

namespace WebAPI.Core.Utils.Security;

/// <summary>Dados do usuário autenticado na requisição atual.</summary>
public class UserInfo
{
    public Guid UserId { get; }

    public string? Login { get; }

    public string? Email { get; }

    public string? Name { get; }

    public string? Role { get; }

    public UserInfo(IHttpContextAccessor accessor)
    {
        Login = GetClaim(ClaimTypes.NameIdentifier, accessor);
        Email = GetClaim(ClaimTypes.Email, accessor);
        Name = GetClaim(ClaimTypes.Name, accessor);
        Role = GetClaim(ClaimTypes.Role, accessor);
        UserId = Guid.TryParse(GetClaim(ClaimTypes.Sid, accessor), out Guid id) ? id : Guid.Empty;
    }

    private static string? GetClaim(string claimType, IHttpContextAccessor accessor) =>
        accessor.HttpContext?.User.FindFirstValue(claimType);
}
