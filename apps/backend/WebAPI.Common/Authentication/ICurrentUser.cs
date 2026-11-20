namespace WebAPI.Common.Authentication;

/// <summary>Usuário da requisição atual (preenchido a partir do JWT pela WebAPI).</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? Name { get; }
}
