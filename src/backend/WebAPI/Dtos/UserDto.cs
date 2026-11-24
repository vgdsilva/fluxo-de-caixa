namespace WebAPI.Dtos;

public sealed record UserDto(Guid Id, string? Name, string? Login, string? Email, string? Role)
{
    public static UserDto From(User user) => new(user.Id, user.Name, user.Login, user.Email, user.Role.ToString());
}
