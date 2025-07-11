using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Tag;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Tag;

public class CreateTagUseCase : ICreateTagUseCase
{
    private readonly ITagRepository _tagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;

    public CreateTagUseCase(
        ITagRepository tagRepository,
        IAuditLogRepository auditLogRepository,
        ILoggedUserService loggedUserService)
    {
        _tagRepository = tagRepository;
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task<TagResponse> ExecuteAsync(TagRequest request, ClaimsPrincipal user)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
            throw new InvalidOperationException("Apenas administradores ou tenants podem criar tags.");

        var tenantId = user.FindFirst("TenantId")?.Value
            ?? throw new InvalidOperationException("TenantId não encontrado no token.");

        var existingTag = await _tagRepository.GetByNameAsync(request.Name, tenantId);
        if (existingTag != null)
            throw new InvalidOperationException("Tag já registrada para este tenant.");

        var tag = new Domain.Entities.Tag
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name
        };

        await _tagRepository.AddAsync(tag);

        var userId = (await _loggedUserService.GetLoggedUserAsync(user)).Id;
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = "Criação de Tag",
            EntityId = tag.Id,
            Details = $"Tag {tag.Name} criada para tenant {tag.TenantId}.",
            CreatedAt = DateTime.UtcNow
        });

        return new TagResponse
        {
            Id = tag.Id,
            TenantId = tag.TenantId,
            Name = tag.Name
        };
    }
}