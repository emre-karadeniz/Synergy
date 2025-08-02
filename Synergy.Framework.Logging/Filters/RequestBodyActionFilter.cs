using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Synergy.Framework.Logging.Options;
using System.Text.Json;

namespace Synergy.Framework.Logging.Filters;

public class RequestBodyActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        if (controllerActionDescriptor is not null)
        {
            var requestBody = FormatRequestBody(context.ActionArguments!);
            context.HttpContext.Items["RequestBody"] = requestBody;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        //throw new NotImplementedException();
    }

    private string FormatRequestBody(IDictionary<string, object> actionArguments)
    {
        if (actionArguments is not null)
        {
            var filteredAtguments = actionArguments
                .Where(kv => kv.Value is not CancellationToken) //CancellationToken olanları alma
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return $"{JsonSerializer.Serialize(filteredAtguments, LoggingJsonSerializerOptions.Cached)}";
        }
        return "";
    }
}
