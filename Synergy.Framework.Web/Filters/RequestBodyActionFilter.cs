using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Synergy.Framework.Web.Extensions;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Synergy.Framework.Web.Filters;

public class RequestBodyActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

        if (controllerActionDescriptor is not null)
        {
            var requestBody = FormatRequestBody(context.ActionArguments!);
            context.HttpContext.SetValue("RequestBody", requestBody);
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

            var options = new JsonSerializerOptions
            {
                WriteIndented = false, //JSON'un minify edilmiş olarak kaydedilmesini sağlar
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, //JSON çıktısı camelCase olur
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping //Escape karakterlerini kaldırır
            };

            return $"{JsonSerializer.Serialize(filteredAtguments, options)}";
        }
        return "";
    }
}
