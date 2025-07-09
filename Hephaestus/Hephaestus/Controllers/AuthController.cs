using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hephaestus.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILoginUseCase _loginUseCase;
    private readonly IRegisterCompanyUseCase _registerCompanyUseCase;

    public AuthController(ILoginUseCase loginUseCase, IRegisterCompanyUseCase registerCompanyUseCase)
    {
        _loginUseCase = loginUseCase;
        _registerCompanyUseCase = registerCompanyUseCase;
    }

    /// <summary>
    /// Autentica uma empresa ou administrador, retornando um token JWT.
    /// </summary>
    /// <param name="request">Dados de login (e-mail e senha).</param>
    /// <returns>Token JWT se as credenciais forem válidas.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var token = await _loginUseCase.ExecuteAsync(request);
            return Ok(new LoginResponse(token));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Registra uma nova empresa, permitido apenas para administradores.
    /// </summary>
    /// <param name="request">Dados da empresa a ser registrada.</param>
    /// <returns>ID da empresa criada.</returns>
    //[Authorize(Roles = "Admin")]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterCompanyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyRequest request)
    {
        try
        {
            var companyId = await _registerCompanyUseCase.ExecuteAsync(request);
            return Created($"/api/company/{companyId}", new RegisterCompanyResponse(companyId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
