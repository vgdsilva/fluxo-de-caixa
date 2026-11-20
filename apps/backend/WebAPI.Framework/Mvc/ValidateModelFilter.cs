using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Framework.Mvc;

/// <summary>Garante resposta ValidationProblemDetails padronizada quando o ModelState é inválido.</summary>
public sealed class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState));
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
