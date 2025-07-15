using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Additional;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Hephaestus.Controllers;

/// <summary>
/// Controller para gerenciamento de adicionais (itens extras) para um tenant.
/// Todas as operações requerem autenticação com a role "Tenant".
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Tenant")]
public class AdditionalController : ControllerBase
{
    private readonly ICreateAdditionalUseCase _createAdditionalUseCase;
    private readonly IGetAdditionalsUseCase _getAdditionalsUseCase;
    private readonly IGetAdditionalByIdUseCase _getAdditionalByIdUseCase;
    private readonly IUpdateAdditionalUseCase _updateAdditionalUseCase;
    private readonly IDeleteAdditionalUseCase _deleteAdditionalUseCase;
    private readonly ILogger<AdditionalController> _logger;

    /// <summary>
    /// Inicializa uma nova instância do <see cref="AdditionalController"/>.
    /// </summary>
    /// <param name="createAdditionalUseCase">Caso de uso para criação de adicionais.</param>
    /// <param name="getAdditionalsUseCase">Caso de uso para listagem de adicionais.</param>
    /// <param name="getAdditionalByIdUseCase">Caso de uso para obtenção de adicional por ID.</param>
    /// <param name="updateAdditionalUseCase">Caso de uso para atualização de adicionais.</param>
    /// <param name="deleteAdditionalUseCase">Caso de uso para remoção de adicionais.</param>
    /// <param name="logger">Logger para registro de eventos.</param>
    public AdditionalController(
        ICreateAdditionalUseCase createAdditionalUseCase,
        IGetAdditionalsUseCase getAdditionalsUseCase,
        IGetAdditionalByIdUseCase getAdditionalByIdUseCase,
        IUpdateAdditionalUseCase updateAdditionalUseCase,
        IDeleteAdditionalUseCase deleteAdditionalUseCase,
        ILogger<AdditionalController> logger)
    {
        _createAdditionalUseCase = createAdditionalUseCase;
        _getAdditionalsUseCase = getAdditionalsUseCase;
        _getAdditionalByIdUseCase = getAdditionalByIdUseCase;
        _updateAdditionalUseCase = updateAdditionalUseCase;
        _deleteAdditionalUseCase = deleteAdditionalUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo adicional para o tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Tenant**.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "name": "Bacon Crocante",
    ///   "description": "Fatias crocantes de bacon",
    ///   "price": 4.99,
    ///   "isAvailable": true
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 201 Created):
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001"
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
    ///     "Name": [
    ///       "O campo 'Name' é obrigatório."
    ///     ]
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Dados do adicional a ser criado.</param>
    /// <returns>Um `CreatedAtActionResult` contendo o ID do novo adicional.</returns>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo adicional", Description = "Cria um novo adicional associado ao tenant do usuário autenticado.")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(object))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAdditional([FromBody] CreateAdditionalRequest request)
    {
        var id = await _createAdditionalUseCase.ExecuteAsync(request, User);
        return CreatedAtAction(nameof(GetAdditionalById), new { id }, new { id });
    }

    /// <summary>
    /// Lista todos os adicionais pertencentes ao tenant autenticado.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Tenant**.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// [
    ///   {
    ///     "id": "123e4567-e89b-12d3-a456-426614174001",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Queijo Extra",
    ///     "description": "Adicional de queijo",
    ///     "price": 3.50,
    ///     "isAvailable": true
    ///   },
    ///   {
    ///     "id": "a7b8c9d0-e1f2-3g4h-5i6j-7k8l9m0n1o2p",
    ///     "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///     "name": "Borda Recheada",
    ///     "description": "Borda com cheddar e catupiry",
    ///     "price": 8.00,
    ///     "isAvailable": false
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <returns>Uma lista de objetos `AdditionalResponse`.</returns>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista adicionais do tenant", Description = "Retorna uma lista paginada de adicionais do tenant autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<AdditionalResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAdditionals(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        var result = await _getAdditionalsUseCase.ExecuteAsync(User, pageNumber, pageSize, sortBy, sortOrder);
        return Ok(result);
    }

    /// <summary>
    /// Obtém os detalhes de um adicional específico pelo seu ID.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Tenant**. O adicional deve pertencer ao tenant autenticado.
    /// 
    /// Exemplo de resposta de sucesso (Status 200 OK):
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174001",
    ///   "tenantId": "456e7890-e89b-12d3-a456-426614174002",
    ///   "name": "Queijo Extra",
    ///   "description": "Adicional de queijo",
    ///   "price": 3.50,
    ///   "isAvailable": true
    /// }
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "message": "Adicional com ID 'xyz' não encontrado para o tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do adicional (GUID).</param>
    /// <returns>Um objeto `AdditionalResponse` contendo os detalhes do adicional.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém adicional por ID", Description = "Retorna os detalhes de um adicional específico pelo seu ID, se ele pertencer ao tenant do usuário autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AdditionalResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAdditionalById(string id)
    {
        var additional = await _getAdditionalByIdUseCase.ExecuteAsync(id, User);
        return Ok(additional);
    }

    /// <summary>
    /// Atualiza as informações de um adicional existente.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Tenant**. O adicional deve pertencer ao tenant autenticado.
    /// 
    /// Exemplo de requisição:
    /// ```json
    /// {
    ///   "name": "Bacon Premium",
    ///   "description": "Fatias de bacon artesanal",
    ///   "price": 6.50,
    ///   "isAvailable": true
    /// }
    /// ```
    /// 
    /// Exemplo de resposta de sucesso (Status 204 No Content):
    /// ```
    /// (Nenhum corpo de resposta)
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "message": "Adicional com ID 'xyz' não encontrado para o tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do adicional a ser atualizado (GUID).</param>
    /// <param name="request">Dados atualizados do adicional.</param>
    /// <returns>Um `NoContentResult` em caso de sucesso.</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza um adicional", Description = "Atualiza as informações de um adicional existente, se ele pertencer ao tenant do usuário autenticado.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAdditional(string id, [FromBody] UpdateAdditionalRequest request)
    {
        await _updateAdditionalUseCase.ExecuteAsync(id, request, User);
        return NoContent();
    }

    /// <summary>
    /// Remove um adicional existente.
    /// </summary>
    /// <remarks>
    /// Requer **Role: Tenant**. O adicional deve pertencer ao tenant autenticado.
    /// 
    /// Exemplo de resposta de sucesso (Status 204 No Content):
    /// ```
    /// (Nenhum corpo de resposta)
    /// ```
    /// 
    /// Exemplo de erro (Status 404 Not Found):
    /// ```json
    /// {
    ///   "message": "Adicional com ID 'xyz' não encontrado para o tenant."
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID do adicional a ser removido (GUID).</param>
    /// <returns>Um `NoContentResult` em caso de sucesso.</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove um adicional", Description = "Remove um adicional existente, se ele pertencer ao tenant do usuário autenticado.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAdditional(string id)
    {
        await _deleteAdditionalUseCase.ExecuteAsync(id, User);
        return NoContent();
    }
}