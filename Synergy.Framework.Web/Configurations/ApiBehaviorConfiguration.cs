using Microsoft.AspNetCore.Mvc;
using Synergy.Framework.Web.Results;

namespace Synergy.Framework.Web.Configurations;

public static class ApiBehaviorConfiguration
{
    /// <summary>
    /// ASP.NET Core'un API davranışını yapılandırır.
    /// Model binding hatalarını ve diğer varsayılan API hatalarını özelleştirmenize olanak tanır.
    /// </summary>
    /// <param name="options">ApiBehaviorOptions örneği.</param>
    /// <param name="useSynergyResultForModelErrors">Model hataları için Synergy Result türünü kullanılıp kullanılmayacağını belirtir. Varsayılan true.</param>
    /// <param name="customInvalidModelStateResponseFactory">Model hataları için özel bir yanıt fabrikası sağlamak üzere isteğe bağlı bir Action.</param>
    public static void ConfigureSynergyApiBehavior(
        ApiBehaviorOptions options,
        bool useSynergyResultForModelErrors = true,
        Func<ActionContext, IActionResult>? customInvalidModelStateResponseFactory = null)
    {
        if (customInvalidModelStateResponseFactory != null)
        {
            // Kullanıcı özel bir fabrika sağladıysa, onu kullan.
            options.InvalidModelStateResponseFactory = customInvalidModelStateResponseFactory;
            return; // Diğer ayarları uygulama.
        }

        if (useSynergyResultForModelErrors)
        {
            // Varsayılan olarak Synergy Result türünü kullan.
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                    .SelectMany(kvp =>
                        kvp.Value!.Errors.Select(err =>
                            string.IsNullOrWhiteSpace(kvp.Key) ? err.ErrorMessage : $"{kvp.Key}: {err.ErrorMessage}"
                        )
                    ).ToList();

                var result = Result.BadRequest(errors);
                return new BadRequestObjectResult(result);
            };
        }
        else
        {
            options.InvalidModelStateResponseFactory = null; // Varsayılan ASP.NET Core davranışını etkinleştirir.
        }
    }
}
