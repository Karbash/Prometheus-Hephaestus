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
    /// Cria uma nova tag para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um **Administrador** ou um **Tenant** crie uma nova tag,
    /// que pode ser associada a itens do card�pio para categoriza��o.
    ///
    /// **Exemplo de Corpo da Requisi��o:**
    /// ```json
    /// {
    ///   "name": "Vegetariano"
    /// }
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 200 OK):**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Vegetariano"
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Valida��o (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "One or more validation errors occurred.",
    ///   "status": 400,
    ///   "errors": {
    ///     "Name": [
    ///       "O campo 'Name' � obrigat�rio e deve ter entre 3 e 50 caracteres."
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
    ///   "detail": "A tag com o nome 'Vegetariano' j� est� registrada para este tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token de autentica��o ou token inv�lido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao criar a tag."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados da tag a ser criada (<see cref="TagRequest"/>).</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo o objeto <see cref="TagResponse"/> da tag criada.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria uma nova tag", Description = "Cria uma nova tag para o tenant autenticado. Requer autentica��o de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TagResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))] // Adicionado para indicar conflito (tag j� existe)
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest request)
    {
        var tagResponse = await _createTagUseCase.ExecuteAsync(request, User);
        return Ok(tagResponse);
    }

    /// GetTags

    /// <summary>
    /// Lista todas as tags associadas ao tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Este endpoint permite que um **Administrador** ou um **Tenant** consulte as tags
    /// registradas para o tenant autenticado, com suporte a pagina��o.
    ///
    /// **Exemplo de Requisi��o:**
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
    ///       "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "name": "Vegetariano"
    ///     },
    ///     {
    ///       "id": "789e0123-e89b-12d3-a456-426614174003",
    ///       "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///       "name": "Sem Gl�ten"
    ///     }
    ///   ],
    ///   "totalCount": 2,
    ///   "pageNumber": 1,
    ///   "pageSize": 20
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "Acesso n�o autorizado."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro Interno do Servidor (Status 500 Internal Server Error):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.6.1](https://tools.ietf.org/html/rfc7231#section-6.6.1)",
    ///   "title": "Internal Server Error",
    ///   "status": 500,
    ///   "detail": "Ocorreu um erro inesperado ao listar as tags."
    /// }
    /// ```
    /// </remarks>
    /// <param name="pageNumber">N�mero da p�gina a ser retornada (padr�o: 1).</param>
    /// <param name="pageSize">N�mero de itens por p�gina (padr�o: 20).</param>
    /// <returns>Um <see cref="OkObjectResult"/> contendo um <see cref="PagedResult{TagResponse}"/> com a lista paginada de tags.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todas as tags do tenant autenticado", Description = "Retorna todas as tags associadas ao tenant do usu�rio autenticado, com suporte a pagina��o. Requer autentica��o de administrador ou tenant.")]
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
    /// do sistema. A tag s� pode ser exclu�da se n�o estiver associada a nenhum item de card�pio.
    ///
    /// **Exemplo de Requisi��o:**
    /// ```http
    /// DELETE /api/Tag/123e4567-e89b-12d3-a456-426614174001
    /// ```
    ///
    /// **Exemplo de Resposta de Sucesso (Status 204 No Content):**
    /// ```
    /// (Nenhum corpo de resposta)
    /// ```
    ///
    /// **Exemplo de Erro de Requisi��o Inv�lida (Status 400 Bad Request):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
    ///   "title": "Bad Request",
    ///   "status": 400,
    ///   "detail": "O ID da tag 'abc-123' n�o � um GUID v�lido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Autoriza��o (Status 401 Unauthorized):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7235#section-3.1](https://tools.ietf.org/html/rfc7235#section-3.1)",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "TenantId n�o encontrado no token de autentica��o ou token inv�lido."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro N�o Encontrado (Status 404 Not Found):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.4](https://tools.ietf.org/html/rfc7231#section-6.5.4)",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "Tag com ID '99999999-9999-9999-9999-999999999999' n�o encontrada ou n�o pertence a este tenant."
    /// }
    /// ```
    ///
    /// **Exemplo de Erro de Conflito (Status 409 Conflict):**
    /// ```json
    /// {
    ///   "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.8](https://tools.ietf.org/html/rfc7231#section-6.5.8)",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "A tag 'Vegetariano' n�o pode ser exclu�da pois est� associada a um ou mais itens de card�pio."
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
    /// <param name="id">O **ID (GUID)** da tag a ser exclu�da.</param>
    /// <returns>Um <see cref="NoContentResult"/> indicando o sucesso da exclus�o.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Exclui uma tag", Description = "Exclui uma tag pelo ID, desde que n�o esteja associada a itens do card�pio. Requer autentica��o de administrador ou tenant.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))] // Para ID inv�lido
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))] // Para tags em uso
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeleteTag(string id)
    {
        await _deleteTagUseCase.ExecuteAsync(id, User);
        return NoContent();
    }
}
