using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Synergy.Framework.Web.Providers;

public class HttpUserIdentifierProvider<TUserKey> : IUserIdentifierProvider<TUserKey>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserIdentifierProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TUserKey UserId
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null || context.User?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("User is not authenticated or HttpContext is null.");

            // Claim tipi: "sub", "id", "userId" olabilir — özelleştirilebilir  
            var claim = context.User.FindFirst("sub") ?? context.User.FindFirst("userId") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new InvalidOperationException("User identifier claim is not found.");

            try
            {
                return (TUserKey)Convert.ChangeType(claim.Value, typeof(TUserKey))!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert claim value to type {typeof(TUserKey)}.", ex);
            }
        }
    }
}
