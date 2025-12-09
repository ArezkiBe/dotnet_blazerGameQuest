using Microsoft.AspNetCore.Mvc;

namespace MySecureApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly KeycloakAuthService _keycloakAuthService;

    private readonly ILogger<AuthController> _logger;

    public AuthController(KeycloakAuthService keycloakAuthService, ILogger<AuthController> logger)
    {
        _keycloakAuthService = keycloakAuthService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login attempt for user: {request.Username}");
            var token = await _keycloakAuthService.LoginAsync(request.Username, request.Password);
            return Ok(token);
        }
        catch (Exception ex)
        {
            return Unauthorized(ex.Message);
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}