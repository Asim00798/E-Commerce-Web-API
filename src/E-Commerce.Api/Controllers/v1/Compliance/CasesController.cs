using E_Commerce.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace E_Commerce.Api.Controllers.v1.Compliance;

[ApiVersion("1.0")]
public class CasesController : BaseApiController
{
    [HttpGet]
    public IActionResult Get() => Ok("Compliance cases endpoint stub");
}
