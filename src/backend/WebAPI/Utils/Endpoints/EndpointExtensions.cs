using FluentValidation;
using Microsoft.AspNetCore.OpenApi;

namespace WebAPI.Utils.Endpoints;

public static class EndpointExtensions
{
    /// <summary>
    /// Grupo raiz <c>/api</c>: toda rota abaixo exige usuário autenticado (quem é público usa
    /// <c>.AllowAnonymous()</c>) e responde sem cache.
    /// </summary>
    public static RouteGroupBuilder MapApi(this IEndpointRouteBuilder app) =>
        app.MapGroup("/api")
           .RequireAuthorization()
           .AddEndpointFilter<NoCacheFilter>();

    /// <summary>
    /// Acha toda classe que implementa <see cref="IEndpoint"/> no projeto e registra no grupo <c>/api</c>
    /// (como o Spring acha os @RestController). Endpoint novo não precisa ser listado em lugar nenhum.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder api)
    {
        IEnumerable<Type> endpoints = typeof(IEndpoint).Assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false });

        foreach (Type type in endpoints)
            ((IEndpoint)Activator.CreateInstance(type)!).Map(api);

        return api;
    }

    /// <summary>
    /// Roda o <see cref="IValidator{T}"/> do request antes do handler e responde 400 (ValidationProblem)
    /// se houver erro. A classe <c>Validation</c> do endpoint é achada pelo DI.
    /// </summary>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValidationFilter<TRequest>>()
               .ProducesValidationProblem();

    /// <summary>
    /// No OpenAPI, um tipo aninhado vira "Endpoint + Tipo" (Login.Request → LoginRequest), para que
    /// os vários <c>Request</c>/<c>Response</c> não colidam no Scalar.
    /// </summary>
    public static OpenApiOptions UseNestedTypeNames(this OpenApiOptions options)
    {
        options.CreateSchemaReferenceId = info =>
        {
            Type type = info.Type;
            if (type.IsNested && type.DeclaringType is { } parent && !type.IsGenericType)
                return parent.Name + type.Name;

            return OpenApiOptions.CreateDefaultSchemaReferenceId(info);
        };
        return options;
    }
}

/// <summary>Filtro de endpoint que valida o argumento do tipo <typeparamref name="TRequest"/>.</summary>
public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        TRequest? request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
            return TypedResults.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Corpo da requisição ausente.");

        FluentValidation.Results.ValidationResult result =
            await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary(), title: "Dados inválidos.");

        return await next(context);
    }
}
