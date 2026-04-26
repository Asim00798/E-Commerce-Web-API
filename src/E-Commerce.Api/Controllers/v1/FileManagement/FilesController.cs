using E_Commerce.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace E_Commerce.Api.Controllers.v1.FileManagement;

[ApiVersion("1.0")]
public class FilesController : BaseApiController
{
    [HttpGet]
    public IActionResult Get() => Ok("File management endpoint stub");
}
