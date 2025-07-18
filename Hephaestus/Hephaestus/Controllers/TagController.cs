using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Interfaces.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de tags, permitindo a cria��o, listagem e exclus�o de tags
/// associadas aos itens de card�pio de um tenant.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Tenant")] // Autoriza��o para Admin ou Tenant
public class TagController : ControllerBase
{
    private readonly ICreateTagUseCase _createTagUseCase;
    private readonly IGetAllTagsByTenantUseCase _getAllTagsByTenantUseCase;
    private readonly IDeleteTagUseCase _deleteTagUseCase;
    private readonly ILogger<TagController> _logger;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="TagController"/>.
    /// </summary>
    /// <param name="createTagUseCase">Caso de uso para cria��o de tags.</param>
    /// <param name="getAllTagsByTenantUseCase">Caso de uso para listagem de tags por tenant.</param>
    /// <param name="deleteTagUseCase">Caso de uso para remo��o de tags.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public TagController(
        ICreateTagUseCase createTagUseCase,
        IGetAllTagsByTenantUseCase getAllTagsByTenantUseCase,
        IDeleteTagUseCase deleteTagUseCase,
        ILogger<TagController> logger)
    {
        _createTagUseCase = createTagUseCase;
        _getAllTagsByTenantUseCase = getAllTagsByTenantUseCase;
        _deleteTagUseCase = deleteTagUseCase;
        _logger = logger;
    }

    /// CreateTag

    /// <summary>
    /// Cria uma nova tag com funcionalidade híbrida.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um **Administrador** ou um **Tenant** crie uma nova tag,
    /// com comportamento diferente baseado no role do usuário:
    ///
    /// **Comportamento por Role:**
    /// - **Admin**: Cria tags **globais** (disponíveis para todas as empresas)
    /// - **Tenant**: Cria tags **locais** (apenas para sua empresa)
    ///
    /// **Exemplo de Corpo da Requisição:**
    /// ```json
    /// {
    ///   "name": "Vegetariano"
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK) - Tag Local:**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "companyId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Vegetariano",
    ///   "isGlobal": false
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK) - Tag Global:**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "companyId": "",
    ///   "name": "Vegetariano",
    ///   "isGlobal": true
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Validação (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Name": [
    ///       "O campo 'Name' é obrigatório e deve ter entre 3 e 50 caracteres."
    ///     ]
    ///   }
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Conflito (Status 409 Conflict):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "Tag já registrada para este tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Operação Inválida (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "error": {
    ///     "code": "INVALID_OPERATION",
    ///     "message": "CompanyId não encontrado no token.",
    ///     "type": "InvalidOperationError"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da tag a ser criada (<see cref="TagRequest"/>).</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo o objeto <see cref="TagResponse"/> da tag criada.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria uma nova tag (híbrida)", Description = "Cria uma nova tag com funcionalidade híbrida: Admin cria tags globais, Tenant cria tags locais. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest request)
    {
        var tagResponse = await _createTagUseCase.ExecuteAsync(request, User);
        return Ok(tagResponse);
    }

    /// GetTags

    /// <summary>
    /// Lista todas as tags disponíveis para o tenant autenticado (híbridas).
    /// </summary>
    /// <remarks>
    /// Este endpoint retorna uma lista híbrida de tags disponíveis para o tenant:
    /// - **Tags Locais**: Criadas pela própria empresa
    /// - **Tags Globais**: Criadas por administradores (disponíveis para todas as empresas)
    ///
    /// **Exemplo de Requisição:**
    /// ```http
    /// GET /api/Tag?pageNumber=1&pageSize=10
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// {
    ///   "items": [
    ///     {
    ///       "id": "123e4567-e89b-12d3-a456-426614174001",
    ///       "companyId": "",
    ///       "name": "Vegetariano",
    ///       "isGlobal": true
    ///     },
    ///     {
    ///       "id": "789e0123-e89b-12d3-a456-426614174003",
    ///       "companyId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "name": "Especial da Casa",
    ///       "isGlobal": false
    ///     }
    ///   ],
    ///   "totalCount": 2,
    ///   "pageNumber": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "Acesso não autorizado."
    /// }
    /// ```
    /// </remarks>
    /// <param name="pageNumber">Número da página a ser retornada (padrão: 1).</param>
    /// <param name="pageSize">Número de itens por página (padrão: 20).</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo um <see cref="PagedResult{TagResponse}"/> com a lista paginada de tags híbridas.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista tags híbridas do tenant", Description = "Retorna tags locais da empresa + tags globais criadas por administradores. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<TagResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetTags(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var tags = await _getAllTagsByTenantUseCase.ExecuteAsync(User, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(tags);
    }

    /// DeleteTag

    /// <summary>
    /// Exclui uma tag existente.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um **Administrador** ou um **Tenant** remova uma tag
    /// do sistema. A tag só pode ser excluída se não estiver associada a nenhum item de cardápio.
    ///
    /// **Exemplo de Requisição:**
    /// ```http
    /// DELETE /api/Tag/123e4567-e89b-12d3-a456-426614174001
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 204 No Content):**
    /// ```
    /// (Nenhum corpo de resposta)
    /// ```
    ///
    /// **Exemplo de Erro de Requisição Inválida (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID da tag 'abc-123' não é um GUID válido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autorização (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId não encontrado no token de autenticação ou token inválido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Não Encontrado (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Tag com ID '99999999-9999-9999-9999-999999999999' não encontrada ou não pertence a este tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Conflito (Status 409 Conflict):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "A tag 'Vegetariano' não pode ser excluída pois está associada a um ou mais itens de cardápio."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao excluir a tag."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID da tag a ser excluída.</param>
    /// <returns>Status 204 No Content em caso de sucesso.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Exclui uma tag", Description = "Exclui uma tag pelo ID, desde que não esteja associada a itens do cardápio. Requer autenticação de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteTag(string id)
    {
        await _deleteTagUseCase.ExecuteAsync(id, User);
        return NoContent();
    }
}
