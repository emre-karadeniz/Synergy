using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Framework.Web.Filters;
using Synergy.Framework.Web.Results;

namespace Synergy.Framework.Web.Base;

[Route("api/[controller]/[action]")]
[ApiController]
[TypeFilter(typeof(RequestBodyActionFilter))]
[Authorize]
public class BaseController: ControllerBase
{
    private IMediator? _mediator;
    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediator>() ?? throw new InvalidOperationException("IMediator service not found.");

    [NonAction]
    public IActionResult CreateActionResult<T>(Result<T> response)
    {
        return StatusCode(response.StatusCode, response);
    }

    [NonAction]
    public IActionResult CreateActionResult(Result response)
    {
        return StatusCode(response.StatusCode, response);
    }
}
