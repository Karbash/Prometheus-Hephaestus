using Hephaestus.Application.Interfaces.Database;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;
using Hephaestus.Domain.Repositories;

namespace Hephaestus.Application.UseCases.Database;

/// <summary>
/// Caso de uso para execu��o de consultas no banco de dados.
/// </summary>
public class ExecuteQueryUseCase : BaseUseCase, IExecuteQueryUseCase
{
    private readonly IDatabaseRepository _databaseRepository;

    /// <summary>
    /// Inicializa uma nova inst�ncia do <see cref="ExecuteQueryUseCase"/>.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Servi�o de tratamento de exce��es.</param>
    /// <param name="databaseRepository">Reposit�rio para execu��o de queries no banco de dados.</param>
    public ExecuteQueryUseCase(
        ILogger<ExecuteQueryUseCase> logger, 
        IExceptionHandlerService exceptionHandler,
        IDatabaseRepository databaseRepository)
        : base(logger, exceptionHandler)
    {
        _databaseRepository = databaseRepository;
    }

    /// <summary>
    /// Executa uma consulta no banco de dados.
    /// </summary>
    /// <param name="request">Requisi��o com a consulta SQL.</param>
    /// <returns>Resultado da consulta.</returns>
    public async Task<ExecuteQueryResponse> ExecuteAsync(ExecuteQueryRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Valida��o dos par�metros de entrada
            ValidateInputParameters(request.Query);

            // Valida��o da seguran�a da consulta
            ValidateQuerySecurity(request.Query);

            // Execu��o da consulta usando o reposit�rio real
            var results = await _databaseRepository.ExecuteQueryAsync(request.Query);

            return new ExecuteQueryResponse
            {
                Results = results
            };
        }, "Execu��o de Consulta SQL");
    }

    /// <summary>
    /// Valida os par�metros de entrada.
    /// </summary>
    /// <param name="query">Consulta SQL.</param>
    private void ValidateInputParameters(string query)
    {
        if (string.IsNullOrEmpty(query))
            throw new Hephaestus.Application.Exceptions.ValidationException("Consulta SQL � obrigat�ria.", new ValidationResult());

        if (query.Length > 10000)
            throw new Hephaestus.Application.Exceptions.ValidationException("Consulta SQL muito longa. M�ximo de 10.000 caracteres permitido.", new ValidationResult());
    }

    /// <summary>
    /// Valida a seguran�a da consulta.
    /// </summary>
    /// <param name="query">Consulta SQL.</param>
    private void ValidateQuerySecurity(string query)
    {
        var upperQuery = query.ToUpperInvariant();

        // Verifica se cont�m comandos perigosos
        var dangerousCommands = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "INSERT", "UPDATE" };
        foreach (var command in dangerousCommands)
        {
            if (upperQuery.Contains(command))
                throw new BusinessRuleException($"Comando '{command}' n�o � permitido por quest�es de seguran�a.", "QUERY_SECURITY");
        }

        // Verifica se � apenas uma consulta SELECT
        if (!upperQuery.TrimStart().StartsWith("SELECT"))
            throw new BusinessRuleException("Apenas consultas SELECT s�o permitidas por quest�es de seguran�a.", "QUERY_SECURITY");
    }
}
