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
    /// <exception cref="InvalidOperationException">Credenciais inválidas ou código MFA inválido.</exception>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Autentica um usuário", Description = "Autentica um usuário com e-mail e senha, retornando um token JWT. Para administradores, um código MFA pode ser necessário.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromQuery] string? mfaCode = null)
    {
        try
        {
            var token = await _loginUseCase.ExecuteAsync(request, mfaCode);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao processar login para {Email}", request?.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar login para {Email}", request?.Email);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// <exception cref="InvalidOperationException">E-mail ou telefone já registrado.</exception>
    [HttpPost("register")]
    [Authorize(Roles = "Admin", Policy = "RequireMfa")]
    [SwaggerOperation(Summary = "Registra uma nova empresa", Description = "Registra uma nova empresa no sistema. Requer autenticação de administrador com MFA validado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyRequest request)
    {
        try
        {
            var companyId = await _registerCompanyUseCase.ExecuteAsync(request, User);
            return Ok(new { companyId });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao registrar empresa com e-mail {Email}", request?.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao registrar empresa com e-mail {Email}", request?.Email);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// <exception cref="InvalidOperationException">E-mail não encontrado.</exception>
    [HttpPost("reset-password-request")]
    [SwaggerOperation(Summary = "Solicita redefinição de senha", Description = "Envia um token de redefinição de senha por e-mail ou WhatsApp para o usuário informado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var token = await _resetPasswordUseCase.RequestResetAsync(request);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao solicitar redefinição de senha para {Email}", request?.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao solicitar redefinição de senha para {Email}", request?.Email);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// <exception cref="InvalidOperationException">Token inválido ou expirado.</exception>
    /// <exception cref="KeyNotFoundException">E-mail não encontrado.</exception>
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Confirma redefinição de senha", Description = "Valida o token de redefinição e atualiza a senha do usuário.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequest request)
    {
        try
        {
            await _resetPasswordUseCase.ConfirmResetAsync(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao confirmar redefinição de senha para {Email}", request?.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao confirmar redefinição de senha para {Email}", request?.Email);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// <exception cref="InvalidOperationException">Apenas administradores podem configurar MFA.</exception>
    [HttpPost("mfa/setup")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Configura MFA", Description = "Gera um segredo TOTP para autenticação multifator de administradores. Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> SetupMfa()
    {
        try
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { error = "E-mail não encontrado no token" });

            var secret = await _mfaUseCase.SetupMfaAsync(email);
            return Ok(new { secret });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao configurar MFA para {Email}", User.FindFirst(ClaimTypes.Email)?.Value);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao configurar MFA para {Email}", User.FindFirst(ClaimTypes.Email)?.Value);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// <exception cref="InvalidOperationException">Código MFA inválido ou usuário não é administrador.</exception>
    [HttpPost("mfa")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Valida código MFA", Description = "Valida o código MFA de um administrador e retorna um novo token JWT com a claim MfaValidated. Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> ValidateMfa([FromBody] MfaRequest request)
    {
        try
        {
            var token = await _mfaUseCase.ExecuteAsync(request);
            return Ok(new { token });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao validar MFA para {Email}", request?.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao validar MFA para {Email}", request?.Email);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
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
    /// <exception cref="InvalidOperationException">Usuário não autenticado ou dados inválidos.</exception>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Obtém dados do usuário logado", Description = "Retorna informações do usuário autenticado com base no token JWT. Requer autenticação.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoggedUser))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(object))]
    public async Task<IActionResult> GetLoggedUser()
    {
        try
        {
            var user = await _loggedUserService.GetLoggedUserAsync(User);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao recuperar usuário logado.");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao recuperar usuário logado.");
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }
}