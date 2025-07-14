using Hephaestus.Application.Interfaces.Database;
using Hephaestus.Application.DTOs.Request;
using Hephaestus.Application.DTOs.Response;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Base;
using Microsoft.Extensions.Logging;
using Hephaestus.Application.Services;
using FluentValidation.Results;

namespace Hephaestus.Application.UseCases.Database;

/// <summary>
/// Caso de uso para execução de consultas no banco de dados.
/// </summary>
public class ExecuteQueryUseCase : BaseUseCase, IExecuteQueryUseCase
{
    /// <summary>
    /// Inicializa uma nova instância do <see cref="ExecuteQueryUseCase"/>.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="exceptionHandler">Serviço de tratamento de exceções.</param>
    public ExecuteQueryUseCase(ILogger<ExecuteQueryUseCase> logger, IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Executa uma consulta no banco de dados.
    /// </summary>
    /// <param name="request">Requisição com a consulta SQL.</param>
    /// <returns>Resultado da consulta.</returns>
    public async Task<ExecuteQueryResponse> ExecuteAsync(ExecuteQueryRequest request)
    {
        return await ExecuteWithExceptionHandlingAsync(async () =>
        {
            // Validação dos parâmetros de entrada
            ValidateInputParameters(request.Query);

            // Validação da segurança da consulta
            ValidateQuerySecurity(request.Query);

            // Execução da consulta
            return await ExecuteQueryAsync(request.Query);
        }, "Execução de Consulta SQL");
    }

    /// <summary>
    /// Valida os parâmetros de entrada.
    /// </summary>
    /// <param name="query">Consulta SQL.</param>
    private void ValidateInputParameters(string query)
    {
        if (string.IsNullOrEmpty(query))
            throw new Hephaestus.Application.Exceptions.ValidationException("Consulta SQL é obrigatória.", new ValidationResult());

        if (query.Length > 10000)
            throw new Hephaestus.Application.Exceptions.ValidationException("Consulta SQL muito longa. Máximo de 10.000 caracteres permitido.", new ValidationResult());
    }

    /// <summary>
    /// Valida a segurança da consulta.
    /// </summary>
    /// <param name="query">Consulta SQL.</param>
    private void ValidateQuerySecurity(string query)
    {
        var upperQuery = query.ToUpperInvariant();

        // Verifica se contém comandos perigosos
        var dangerousCommands = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "INSERT", "UPDATE" };
        foreach (var command in dangerousCommands)
        {
            if (upperQuery.Contains(command))
                throw new BusinessRuleException($"Comando '{command}' não é permitido por questões de segurança.", "QUERY_SECURITY");
        }

        // Verifica se é apenas uma consulta SELECT
        if (!upperQuery.TrimStart().StartsWith("SELECT"))
            throw new BusinessRuleException("Apenas consultas SELECT são permitidas por questões de segurança.", "QUERY_SECURITY");
    }

    /// <summary>
    /// Executa a consulta no banco de dados.
    /// </summary>
    /// <param name="query">Consulta SQL.</param>
    /// <returns>Resultado da consulta.</returns>
    private async Task<ExecuteQueryResponse> ExecuteQueryAsync(string query)
    {
        // Implementação da execução da consulta
        // Esta é uma implementação de exemplo - em um caso real, você conectaria ao banco de dados
        await Task.Delay(100); // Simula processamento assíncrono

        return new ExecuteQueryResponse
        {
            Results = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["Query"] = query,
                    ["Result"] = "Resultado da consulta seria retornado aqui",
                    ["ExecutedAt"] = DateTime.UtcNow,
                    ["Status"] = "Success"
                }
            }
        };
    }
}