using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para autenticação e gerenciamento de usuários.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ILoginUseCase _loginUseCase;
    private readonly IRegisterCompanyUseCase _registerCompanyUseCase;
    private readonly IResetPasswordUseCase _resetPasswordUseCase;
    private readonly IMfaUseCase _mfaUseCase;
    private readonly ILoggedUserService _loggedUserService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="AuthController"/>.
    /// </summary>
    /// <param name="loginUseCase">Caso de uso para login.</param>
    /// <param name="registerCompanyUseCase">Caso de uso para registro de empresas.</param>
    /// <param name="resetPasswordUseCase">Caso de uso para redefinição de senha.</param>
    /// <param name="mfaUseCase">Caso de uso para autenticação multifator (MFA).</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger para registro de erros.</param>
    public AuthController(
        ILoginUseCase loginUseCase,
        IRegisterCompanyUseCase registerCompanyUseCase,
        IResetPasswordUseCase resetPasswordUseCase,
        IMfaUseCase mfaUseCase,
        ILoggedUserService loggedUserService,
        ILogger<AuthController> logger)
    {
        _loginUseCase = loginUseCase;
        _registerCompanyUseCase = registerCompanyUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
        _mfaUseCase = mfaUseCase;
        _loggedUserService = loggedUserService;
        _logger = logger;
    }

    /// <summary>
    /// Autentica um usuário e retorna um token JWT.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Credenciais inválidas."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados de login (e-mail e senha).</param>
    /// <param name="mfaCode">Código MFA opcional para administradores.</param>
    /// <returns>Token JWT.</returns>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Autentica um usuário", Description = "Autentica um usuário com e-mail e senha, retornando um token JWT. Para administradores, um código MFA pode ser necessário.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromQuery] string? mfaCode = null)
    {
        var token = await _loginUseCase.ExecuteAsync(request, mfaCode);
        return Ok(new { token });
    }

    /// <summary>
    /// Registra uma nova empresa (apenas administradores com MFA validado).
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "companyId": "123e4567-e89b-12d3-a456-426614174001"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "E-mail já registrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da empresa a ser registrada.</param>
    /// <returns>ID da empresa registrada.</returns>
    [HttpPost("register")]
    [Authorize(Roles = "Admin", Policy = "RequireMfa")]
    [SwaggerOperation(Summary = "Registra uma nova empresa", Description = "Registra uma nova empresa no sistema. Requer autenticação de administrador com MFA validado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyRequest request)
    {
        var companyId = await _registerCompanyUseCase.ExecuteAsync(request, User);
        return Ok(new { companyId });
    }

    /// <summary>
    /// Solicita um token para redefinição de senha.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "token": "ABC123"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "E-mail não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">E-mail do usuário.</param>
    /// <returns>Token de redefinição de senha.</returns>
    [HttpPost("reset-password-request")]
    [SwaggerOperation(Summary = "Solicita redefinição de senha", Description = "Envia um token de redefinição de senha por e-mail ou WhatsApp para o usuário informado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequest request)
    {
        var token = await _resetPasswordUseCase.RequestResetAsync(request);
        return Ok(new { token });
    }

    /// <summary>
    /// Confirma a redefinição de senha com um token.
    /// </summary>
    /// <remarks>
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Token inválido ou expirado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">E-mail, token e nova senha.</param>
    /// <returns>Status de sucesso.</returns>
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Confirma redefinição de senha", Description = "Valida o token de redefinição e atualiza a senha do usuário.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequest request)
    {
        await _resetPasswordUseCase.ConfirmResetAsync(request);
        return Ok();
    }

    /// <summary>
    /// Configura a autenticação multifator (MFA) para um administrador.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "secret": "JBSWY3DPEHPK3PXP"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Apenas administradores podem configurar MFA."
    /// }
    /// ```
    /// </remarks>
    /// <returns>Segredo TOTP para configuração no aplicativo autenticador.</returns>
    [HttpPost("mfa/setup")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Configura MFA", Description = "Gera um segredo TOTP para autenticação multifator de administradores. Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> SetupMfa()
    {
        var email = GetUserEmail();
        var secret = await _mfaUseCase.SetupMfaAsync(email);
        return Ok(new { secret });
    }

    /// <summary>
    /// Valida um código MFA e retorna um novo token JWT com MFA validado.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Código MFA inválido."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">E-mail e código MFA.</param>
    /// <returns>Novo token JWT com claim MfaValidated.</returns>
    [HttpPost("mfa")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Valida código MFA", Description = "Valida o código MFA de um administrador e retorna um novo token JWT com a claim MfaValidated. Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ValidateMfa([FromBody] MfaRequest request)
    {
        var token = await _mfaUseCase.ExecuteAsync(request);
        return Ok(new { token });
    }

    /// <summary>
    /// Retorna os dados do usuário logado.
    /// </summary>
    /// <remarks>
    /// Exemplo de resposta de sucesso:
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "name": "Admin",
    ///   "email": "admin@example.com",
    ///   "role": "Admin"
    /// }
    /// ```
    /// Exemplo de erro:
    /// ```json
    /// {
    ///   "error": "Usuário não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <returns>Dados do usuário logado (ID, nome, e-mail, função).</returns>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Obtém dados do usuário logado", Description = "Retorna informações do usuário autenticado com base no token JWT. Requer autenticação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoggedUser))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetLoggedUser()
    {
        var user = await _loggedUserService.GetLoggedUserAsync(User);
        return Ok(user);
    }

    /// <summary>
    /// Obtém o e-mail do usuário do token de autenticação.
    /// </summary>
    /// <returns>E-mail do usuário.</returns>
    private string GetUserEmail()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("E-mail não encontrado no token para o usuário {UserId}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            throw new UnauthorizedAccessException("E-mail não encontrado no token");
        }
        return email;
    }
}