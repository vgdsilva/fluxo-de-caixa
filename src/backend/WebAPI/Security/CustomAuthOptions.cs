using Microsoft.AspNetCore.Authentication;

namespace WebAPI.Security;

public class CustomAuthOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "Bearer";

    public string Scheme => DefaultScheme;
}
