using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Modules.Users.Endpoints;

/// <summary>GET /api/me: usuário do JWT atual.</summary>
public static class GetMe
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder group) =>
            group.MapGet("/", Handle)
                 .WithName("GetMe")
                 .WithSummary("Usuário do JWT atual.")
                 .ProducesProblem(StatusCodes.Status401Unauthorized);

        // Sem Request (não há corpo nem parâmetros) e sem Validation; a resposta é o UserResponse do módulo.
        private static Ok<UserResponse> Handle(UserInfo userInfo) =>
            TypedResults.Ok(new UserResponse(userInfo.UserId, userInfo.Name, userInfo.Login, userInfo.Email, userInfo.Role));
    }
}
