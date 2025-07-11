using Hephaestus.Application.Interfaces.Tag;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases.Tag;

public class DeleteTagUseCase : IDeleteTagUseCase
{
    private readonly ITagRepository _tagRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILoggedUserService _loggedUserService;

    public DeleteTagUseCase(
        ITagRepository tagRepository,
        IAuditLogRepository auditLogRepository,
        ILoggedUserService loggedUserService)
    {
        _tagRepository = tagRepository;
        _auditLogRepository = auditLogRepository;
        _loggedUserService = loggedUserService;
    }

    public async Task ExecuteAsync(string id, ClaimsPrincipal user)
    {
        var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin" && userRole != "Tenant")
            throw new InvalidOperationException("Apenas administradores ou tenants podem excluir tags.");

        var tenantId = user.FindFirst("TenantId")?.Value
            ?? throw new InvalidOperationException("TenantId não encontrado no token.");

        var tag = await _tagRepository.GetByIdAsync(id, tenantId);
        if (tag == null)
            throw new InvalidOperationException("Tag não encontrada.");

        if (userRole == "Tenant" && tenantId != tag.TenantId)
            throw new InvalidOperationException("Tenants só podem excluir suas próprias tags.");

        await _tagRepository.DeleteAsync(id, tenantId);

        var userId = (await _loggedUserService.GetLoggedUserAsync(user)).Id;
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = "Exclusão de Tag",
            EntityId = id,
            Details = $"Tag com ID {id} excluída para tenant {tenantId}.",
            CreatedAt = DateTime.UtcNow
        });
    }
}