using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.Interfaces.Auth;
using Hephaestus.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para autenticação de usuários e gerenciamento de contas, incluindo login, registro de empresas, redefinição de senha e MFA.
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
    /// <param name="loginUseCase">Caso de uso para login de usuários.</param>
    /// <param name="registerCompanyUseCase">Caso de uso para registro de novas empresas.</param>
    /// <param name="resetPasswordUseCase">Caso de uso para redefinição de senha.</param>
    /// <param name="mfaUseCase">Caso de uso para autenticação multifator (MFA).</param>
    /// <param name="loggedUserService">Serviço para obter informações do usuário logado.</param>
    /// <param name="logger">Logger para registro de eventos e erros.</param>
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
    /// Para administradores, um **código MFA** pode ser necessário se estiver configurado.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "email": "usuario@exemplo.com",
    ///   "password": "SenhaSegura123!"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// ```
    /// 
    /// Exemplo de erro de credenciais inválidas (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "Credenciais inválidas."
    /// }
    /// ```
    /// Exemplo de erro MFA necessário (Status 401 Unauthorized):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "Código MFA é necessário para este usuário."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados de login: e-mail e senha.</param>
    /// <param name="mfaCode">Código MFA opcional, requerido para administradores com MFA ativo.</param>
    /// <returns>Um `OkResult` contendo o token JWT.</returns>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Autentica um usuário", Description = "Autentica um usuário com e-mail e senha, retornando um token JWT. Administradores podem precisar fornecer um código MFA.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Use ProblemDetails para erros genéricos
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))] // Específico para MFA
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromQuery] string? mfaCode = null)
    {
        var token = await _loginUseCase.ExecuteAsync(request, mfaCode);
        return Ok(new { token });
    }

    /// <summary>
    /// Registra uma nova empresa no sistema.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin** e **MFA validado**.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "companyName": "Nova Empresa SA",
    ///   "email": "contato@novaempresa.com",
    ///   "password": "SenhaForteEmpresa!",
    ///   "phoneNumber": "11987654321",
    ///   "city": "São Paulo",
    ///   "neighborhood": "Centro",
    ///   "street": "Rua das Flores",
    ///   "number": "100"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "companyId": "123e4567-e89b-12d3-a456-426614174001"
    /// }
    /// ```
    /// 
    /// Exemplo de erro de validação (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Email": [
    ///       "O formato do e-mail é inválido."
    ///     ]
    ///   }
    /// }
    /// ```
    /// Exemplo de erro de conflito (Status 409 Conflict):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "E-mail 'contato@novaempresa.com' já registrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da empresa a ser registrada.</param>
    /// <returns>Um `OkResult` contendo o ID da empresa registrada.</returns>
    [HttpPost("register")]
    [Authorize(Roles = "Admin", Policy = "RequireMfa")]
    [SwaggerOperation(Summary = "Registra uma nova empresa", Description = "Registra uma nova empresa no sistema. Esta operação requer autenticação de administrador com MFA validado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterCompanyRequest request)
    {
        var companyId = await _registerCompanyUseCase.ExecuteAsync(request, User);
        return Ok(new { companyId });
    }

    /// <summary>
    /// Solicita um token para iniciar o processo de redefinição de senha.
    /// </summary>
    /// <remarks>
    /// Um token será enviado para o **e-mail** ou **WhatsApp** do usuário.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "email": "usuario@exemplo.com"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "message": "Um token de redefinição de senha foi enviado para seu e-mail/WhatsApp."
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "E-mail 'naoexistente@exemplo.com' não encontrado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">E-mail do usuário que deseja redefinir a senha.</param>
    /// <returns>Um `OkResult` indicando que o token foi enviado.</returns>
    [HttpPost("reset-password-request")]
    [SwaggerOperation(Summary = "Solicita redefinição de senha", Description = "Inicia o processo de redefinição de senha enviando um token para o e-mail ou WhatsApp do usuário.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))] // Para erros de formato de e-mail, por exemplo
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequest request)
    {
        var token = await _resetPasswordUseCase.RequestResetAsync(request);
        return Ok(new { token });
    }

    /// <summary>
    /// Confirma a redefinição de senha utilizando o token recebido.
    /// </summary>
    /// <remarks>
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "email": "usuario@exemplo.com",
    ///   "token": "ABC123XYZ",
    ///   "newPassword": "NovaSenhaSegura456!"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 200)
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "Token inválido ou expirado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">E-mail do usuário, token de redefinição e a nova senha.</param>
    /// <returns>Um `OkResult` em caso de sucesso na redefinição.</returns>
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Confirma redefinição de senha", Description = "Valida o token de redefinição de senha e atualiza a senha do usuário.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Para token inválido/expirado ou senha fraca
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))] // Para email não encontrado (embora o request inicial já evite isso)
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordConfirmRequest request)
    {
        await _resetPasswordUseCase.ConfirmResetAsync(request);
        return Ok();
    }

    /// <summary>
    /// Configura a autenticação multifator (MFA) para um usuário administrador.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin**. Retorna um segredo **TOTP** que deve ser configurado em um aplicativo autenticador (e.g., Google Authenticator).
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "secret": "JBSWY3DPEHPK3PXP"
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "MFA já configurado para este usuário."
    /// }
    /// ```
    /// </remarks>
    /// <returns>Um `OkResult` contendo o segredo TOTP.</returns>
    [HttpPost("mfa/setup")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Configura MFA", Description = "Gera um segredo TOTP para configuração da autenticação multifator para um administrador. Requer autenticação de administrador.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetupMfa()
    {
        var email = GetUserEmail();
        var secret = await _mfaUseCase.SetupMfaAsync(email);
        return Ok(new { secret });
    }

    /// <summary>
    /// Valida um código MFA fornecido por um administrador e retorna um novo token JWT com a claim `MfaValidated`.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Admin**.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "email": "admin@exemplo.com",
    ///   "mfaCode": "123456"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 400 Bad Request):
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "Código MFA inválido."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">E-mail do administrador e o código MFA gerado pelo aplicativo autenticador.</param>
    /// <returns>Um `OkResult` contendo o novo token JWT.</returns>
    [HttpPost("mfa")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Valida código MFA", Description = "Valida o código MFA de um administrador e retorna um novo token JWT com a claim de MFA validado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateMfa([FromBody] MfaRequest request)
    {
        var token = await _mfaUseCase.ExecuteAsync(request);
        return Ok(new { token });
    }

    /// <summary>
    /// Retorna os dados do usuário atualmente logado.
    /// </summary>
    /// <remarks>
    /// Requer autenticação (qualquer role).
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "userId": "dbe2b66a-1d5f-4a7b-8c9e-0f1a2b3c4d5e",
    ///   "email": "usuario@exemplo.com",
    ///   "roles": ["Tenant"],
    ///   "isAuthenticated": true,
    ///   "mfaValidated": false,
    ///   "tenantId": "e1f2g3h4-i5j6-k7l8-m9n0-o1p2q3r4s5t6"
    /// }
    /// ```
    /// 
    /// Exemplo de resposta para usuário não autenticado (Status 401 Unauthorized):
    /// ```
    /// (Nenhum corpo de resposta, apenas status 401)
    /// ```
    /// </remarks>
    /// <returns>Um objeto `LoggedUserResponse` com as informações do usuário.</returns>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(Summary = "Obtém dados do usuário logado", Description = "Retorna informações do usuário autenticado com base no token JWT.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoggedUser))] // Assumindo que LoggedUser é LoggedUserResponse
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLoggedUser()
    {
        var user = await _loggedUserService.GetLoggedUserAsync(User);
        return Ok(user);
    }

    /// <summary>
    /// Obtém o e-mail do usuário do token de autenticação.
    /// </summary>
    /// <returns>O e-mail do usuário como string.</returns>
    /// <exception cref="UnauthorizedAccessException">Lançada se o e-mail não for encontrado no token.</exception>
    private string GetUserEmail()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("E-mail não encontrado no token para o usuário {UserId}",
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            throw new UnauthorizedAccessException("E-mail não encontrado no token.");
        }
        return email;
    }
}