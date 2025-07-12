using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Interfaces.Database;
using Hephaestus.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Database;

public class ExecuteQueryUseCase : IExecuteQueryUseCase
{
    private readonly IDatabaseRepository _databaseRepository;
    private readonly ILogger<ExecuteQueryUseCase> _logger;

    public ExecuteQueryUseCase(IDatabaseRepository databaseRepository, ILogger<ExecuteQueryUseCase> logger)
    {
        _databaseRepository = databaseRepository;
        _logger = logger;
    }

    public async Task<ExecuteQueryResponse> ExecuteAsync(ExecuteQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Query é obrigatória.", nameof(request.Query));

        try
        {
            var results = await _databaseRepository.ExecuteQueryAsync(request.Query);
            return new ExecuteQueryResponse { Results = results };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar query: {Query}", request.Query);
            throw;
        }
    }
}