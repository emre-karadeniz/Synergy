using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synergy.Framework.Web.Base;
using Synergy.Sample.Api.Models;

namespace Synergy.Sample.Api.Controllers
{
    public class TestController : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateCategory(CategoryAddDto request)
        {
            return Ok("başarılı");
            //return CreateActionResult(await Mediator.Send(request));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            return Ok("başarılı");
            //return CreateActionResult(await Mediator.Send(request));
        }

        [HttpGet]        
        public async Task<IActionResult> GetCategories2()
        {
            return Ok("başarılı");
            //return CreateActionResult(await Mediator.Send(request));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById([FromQuery] GetCategoryByIdQuery request)
        {
            return Ok("başarılı");
            //return CreateActionResult(await Mediator.Send(request));
        }
    }
}
