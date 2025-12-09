using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MySecureApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    [HttpGet("get-admin")]
    [Authorize(Roles = "admin")]
    public IActionResult teste()
    {
        return Ok("You are admin.");
    }

    [HttpGet("get-general")]
    [Authorize(Roles = "joueur")]
    public IActionResult GetGeneral()
    {
        return Ok("You are general.");
    }
}
