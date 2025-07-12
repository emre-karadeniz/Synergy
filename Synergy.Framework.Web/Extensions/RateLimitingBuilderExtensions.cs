using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Synergy.Framework.Web.Configurations;
using Synergy.Framework.Web.Results;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Synergy.Framework.Web.Extensions;

/// <summary>
/// API Rate Limiting ayarlarını yapılandırmak için uzantı metotları.
/// </summary>
public static class RateLimitingBuilderExtensions
{
    /// <summary>
    /// Rate Limiting servislerini yapılandırır ve DI konteynerine ekler.
    /// </summary>
    public static IServiceCollection AddSynergyRateLimiting(this IServiceCollection services, Action<RateLimitingOptions>? configureOptions = null)
    {
        var rateLimitingOptions = new RateLimitingOptions();
        // Opsiyonel konfigürasyon uygula
        configureOptions?.Invoke(rateLimitingOptions);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = rateLimitingOptions.RejectionStatusCode;

            // OnRejected event'inde ILoggingService'i kullanmak için factory method.
            // Bu, serviceProvider.GetRequiredService'ı doğrudan burada çağırmaktan daha iyidir.
            options.OnRejected = async (context, token) =>
            {
                // Ratelimiting loglaması ama burada yapmayacaz. Api tarafında yapılabilir
                //using var scope = context.HttpContext.RequestServices.CreateScope();
                //var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
                //loggingService.LogRateLimiting();

                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var response = Result.RateLimiting();
                var json = JsonSerializer.Serialize(response);
                await context.HttpContext.Response.WriteAsync(json, token);
            };

            // Global Limiter Tanımlama
            if (rateLimitingOptions.EnableGlobalLimiting)
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var userId = httpContext.User?.Identity?.IsAuthenticated == true
                        ? httpContext.User.Identity?.Name ?? "anonymous"
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                    var endpoint = httpContext.GetEndpoint()?.DisplayName ?? "default";
                    var key = $"{userId}:{endpoint}";

                    return rateLimitingOptions.GlobalPolicyName switch
                    {
                        // Sabit Pencere
                        "GlobalFixedWindowPolicy" => RateLimitPartition.GetFixedWindowLimiter(key, _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitingOptions.GlobalFixedWindowOptions.PermitLimit,
                                Window = TimeSpan.FromSeconds(rateLimitingOptions.GlobalFixedWindowOptions.WindowSeconds),
                                QueueLimit = rateLimitingOptions.GlobalFixedWindowOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(rateLimitingOptions.GlobalFixedWindowOptions.QueueProcessingOrder)
                            }),
                        // Kayar Pencere
                        "GlobalSlidingWindowPolicy" => RateLimitPartition.GetSlidingWindowLimiter(key, _ =>
                            new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitingOptions.GlobalSlidingWindowOptions.PermitLimit,
                                Window = TimeSpan.FromSeconds(rateLimitingOptions.GlobalSlidingWindowOptions.WindowSeconds),
                                SegmentsPerWindow = rateLimitingOptions.GlobalSlidingWindowOptions.SegmentsPerWindow,
                                QueueLimit = rateLimitingOptions.GlobalSlidingWindowOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(rateLimitingOptions.GlobalSlidingWindowOptions.QueueProcessingOrder)
                            }),
                        // Token Kovası
                        "GlobalTokenBucketPolicy" => RateLimitPartition.GetTokenBucketLimiter(key, _ =>
                            new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = rateLimitingOptions.GlobalTokenBucketOptions.TokenLimit,
                                QueueLimit = rateLimitingOptions.GlobalTokenBucketOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(rateLimitingOptions.GlobalTokenBucketOptions.QueueProcessingOrder),
                                ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitingOptions.GlobalTokenBucketOptions.ReplenishmentPeriodSeconds),
                                TokensPerPeriod = rateLimitingOptions.GlobalTokenBucketOptions.TokensPerPeriod,
                                AutoReplenishment = rateLimitingOptions.GlobalTokenBucketOptions.AutoReplenishment
                            }),
                        // Eşzamanlılık Limiti
                        "GlobalConcurrencyPolicy" => RateLimitPartition.GetConcurrencyLimiter(key, _ =>
                            new System.Threading.RateLimiting.ConcurrencyLimiterOptions
                            {
                                PermitLimit = rateLimitingOptions.GlobalConcurrencyOptions.PermitLimit,
                                QueueLimit = rateLimitingOptions.GlobalConcurrencyOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(rateLimitingOptions.GlobalConcurrencyOptions.QueueProcessingOrder)
                            }),
                        _ => RateLimitPartition.GetFixedWindowLimiter(key, _ => // Varsayılan olarak Fixed Window
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitingOptions.GlobalFixedWindowOptions.PermitLimit,
                                Window = TimeSpan.FromSeconds(rateLimitingOptions.GlobalFixedWindowOptions.WindowSeconds),
                                QueueLimit = rateLimitingOptions.GlobalFixedWindowOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(rateLimitingOptions.GlobalFixedWindowOptions.QueueProcessingOrder)
                            })
                    };
                });
            }

            // Özel Endpoint/User Bazlı Politikalar (Daha gelişmiş kullanım için)
            foreach (var policy in rateLimitingOptions.EndpointPolicies)
            {
                options.AddPolicy(policy.Key, httpContext =>
                {
                    var userId = httpContext.User?.Identity?.IsAuthenticated == true
                        ? httpContext.User.Identity?.Name ?? "anonymous"
                        : httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                    var endpoint = httpContext.GetEndpoint()?.DisplayName ?? "default";
                    var key = $"{userId}:{endpoint}:{policy.Key}"; // Policy adına göre benzersiz anahtar

                    return policy.Value.LimiterType switch
                    {
                        "FixedWindow" => RateLimitPartition.GetFixedWindowLimiter(key, _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = policy.Value.FixedWindowOptions.PermitLimit,
                                Window = TimeSpan.FromSeconds(policy.Value.FixedWindowOptions.WindowSeconds),
                                QueueLimit = policy.Value.FixedWindowOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(policy.Value.FixedWindowOptions.QueueProcessingOrder)
                            }),
                        "SlidingWindow" => RateLimitPartition.GetSlidingWindowLimiter(key, _ =>
                            new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = policy.Value.SlidingWindowOptions.PermitLimit,
                                Window = TimeSpan.FromSeconds(policy.Value.SlidingWindowOptions.WindowSeconds),
                                SegmentsPerWindow = policy.Value.SlidingWindowOptions.SegmentsPerWindow,
                                QueueLimit = policy.Value.SlidingWindowOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(policy.Value.SlidingWindowOptions.QueueProcessingOrder)
                            }),
                        "TokenBucket" => RateLimitPartition.GetTokenBucketLimiter(key, _ =>
                            new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = policy.Value.TokenBucketOptions.TokenLimit,
                                QueueLimit = policy.Value.TokenBucketOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(policy.Value.TokenBucketOptions.QueueProcessingOrder),
                                ReplenishmentPeriod = TimeSpan.FromSeconds(policy.Value.TokenBucketOptions.ReplenishmentPeriodSeconds),
                                TokensPerPeriod = policy.Value.TokenBucketOptions.TokensPerPeriod,
                                AutoReplenishment = policy.Value.TokenBucketOptions.AutoReplenishment
                            }),
                        "Concurrency" => RateLimitPartition.GetConcurrencyLimiter(key, _ =>
                            new System.Threading.RateLimiting.ConcurrencyLimiterOptions
                            {
                                PermitLimit = policy.Value.ConcurrencyOptions.PermitLimit,
                                QueueLimit = policy.Value.ConcurrencyOptions.QueueLimit,
                                QueueProcessingOrder = ParseQueueProcessingOrder(policy.Value.ConcurrencyOptions.QueueProcessingOrder)
                            }),
                        _ => RateLimitPartition.GetNoLimiter<string>(key) // Tanımsız tipte limit yok
                    };
                });
            }
        });

        return services;
    }

    /// <summary>
    /// HTTP request pipeline'ına Rate Limiting middleware'ini ekler.
    /// </summary>
    public static IApplicationBuilder UseSynergyRateLimiting(this IApplicationBuilder app)
    {
        var rateLimitingOptions = app.ApplicationServices.GetRequiredService<IConfiguration>()
            .GetSection(RateLimitingOptions.ConfigurationSectionName)
            .Get<RateLimitingOptions>();

        if (rateLimitingOptions == null || !rateLimitingOptions.EnableGlobalLimiting)
        {
            return app; // Eğer seçenekler okunamazsa veya global limiting kapalıysa, middleware'i ekleme
        }

        // Global Rate Limiting'i etkinleştir
        app.UseRateLimiter();

        return app;
    }

    /// <summary>
    /// appsettings.json'dan okunan string değeri QueueProcessingOrder enum'ına dönüştürür.
    /// </summary>
    private static QueueProcessingOrder ParseQueueProcessingOrder(string order)
    {
        if (Enum.TryParse<QueueProcessingOrder>(order, true, out var parsedOrder))
        {
            return parsedOrder;
        }
        return QueueProcessingOrder.OldestFirst; // Varsayılan
    }
}

//builder.AddSynergyWeb(options =>
//{
//    options.EnableRateLimiting = true;
//    options.RateLimitingOptions = rateOpt =>
//    {
//        rateOpt.EnableGlobalLimiting = true;
//        rateOpt.GlobalPolicyName = "GlobalFixedWindowPolicy"; // veya "GlobalSlidingWindowPolicy", "GlobalTokenBucketPolicy", "GlobalConcurrencyPolicy"
//        rateOpt.GlobalFixedWindowOptions.PermitLimit = 100;
//        rateOpt.GlobalFixedWindowOptions.WindowSeconds = 60;
//        // Diğer ayarlar...

//rateOpt.EndpointPolicies.Add("/api/special", new EndpointRateLimiterOptions
//        {
//            LimiterType = "FixedWindow",
//            FixedWindowOptions = new FixedWindowLimiterOptions
//            {
//                PermitLimit = 5,
//                WindowSeconds = 10
//            }
//        });

//    };
//});

//app.UseSynergyWebMiddlewares();