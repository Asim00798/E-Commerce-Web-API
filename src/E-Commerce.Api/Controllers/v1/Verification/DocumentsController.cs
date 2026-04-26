using E_Commerce.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace E_Commerce.Api.Controllers.v1.Verification;

[ApiVersion("1.0")]
public class DocumentsController : BaseApiController
{
    [HttpGet]
    public IActionResult Get() => Ok("Verification documents endpoint stub");
}
