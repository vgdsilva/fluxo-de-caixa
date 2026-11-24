namespace WebAPI.Modules.Users;

/// <summary>Usuário como aparece nas respostas da API (login, refresh e /api/me).</summary>
public sealed record UserResponse(Guid Id, string? Name, string? Login, string? Email, string? Role)
{
    public static UserResponse From(User user) =>
        new(user.Id, user.Name, user.Login, user.Email, user.Role.ToString());
}
