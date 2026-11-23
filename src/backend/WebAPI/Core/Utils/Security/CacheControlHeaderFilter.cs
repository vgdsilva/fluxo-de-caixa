using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Core.Utils.Security;

public class CacheControlHeaderFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
    {
        base.OnActionExecuted(actionExecutedContext);
        var headers = actionExecutedContext.HttpContext.Response.Headers;
        headers.CacheControl = "no-cache, no-store, must-revalidate";
        headers.Pragma = "no-cache";
        headers.Expires = "0";
    }
}
