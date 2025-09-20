using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagementSystem.Configs;
using System.Security.Claims;

namespace ProductManagementSystem.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        protected string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        protected string GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        protected string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        protected IActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                Helpers.ApiException apiEx => StatusCode(apiEx.StatusCode, new ResponseEntity(apiEx.StatusCode, apiEx.Message)),
                _ => StatusCode(500, new ResponseEntity(500, "An unexpected error occurred"))
            };
        }
    }
}
