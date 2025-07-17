using Hephaestus.Application.UseCases.Review;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace Hephaestus.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly CreateReviewUseCase _createReviewUseCase;

    public ReviewController(CreateReviewUseCase createReviewUseCase)
    {
        _createReviewUseCase = createReviewUseCase;
    }

    /// <summary>
    /// Cria um novo review para um pedido.
    /// </summary>
    /// <param name="request">Dados do review.</param>
    /// <returns>Review criado.</returns>
    [HttpPost]
    public async Task<ActionResult<ReviewResponse>> Create([FromBody] CreateReviewRequest request)
    {
        var response = await _createReviewUseCase.ExecuteAsync(request);
        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }
} 