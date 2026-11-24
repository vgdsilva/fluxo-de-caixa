using Microsoft.AspNetCore.Http.HttpResults;

namespace WebAPI.Endpoints.Me;

/// <summary>GET /api/me: usuário do JWT atual.</summary>
public static class GetMe
{
    public sealed class Endpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder api) =>
            api.MapGet("/me", Handle)
                 .WithTags("Me")
                 .WithName("GetMe")
                 .WithSummary("Usuário do JWT atual.")
                 .ProducesProblem(StatusCodes.Status401Unauthorized);

        // Sem Request (não há corpo nem parâmetros) e sem Validation; a resposta é o UserDto, que também vai no login.
        private static Ok<UserDto> Handle(UserInfo userInfo) =>
            TypedResults.Ok(new UserDto(userInfo.UserId, userInfo.Name, userInfo.Login, userInfo.Email, userInfo.Role));
    }
}
