using FluentValidation;
using FluentValidation.Results;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Microsoft.Extensions.Logging;

namespace Hephaestus.Application.Base;

/// <summary>
/// Classe base para todos os use cases com tratamento de exceções.
/// </summary>
public abstract class BaseUseCase
{
    protected readonly ILogger Logger;
    protected readonly IExceptionHandlerService ExceptionHandler;

    protected BaseUseCase(ILogger logger, IExceptionHandlerService exceptionHandler)
    {
        Logger = logger;
        ExceptionHandler = exceptionHandler;
    }

    /// <summary>
    /// Executa uma validação e lança uma exceção apropriada se falhar.
    /// </summary>
    /// <typeparam name="T">Tipo do objeto a ser validado.</typeparam>
    /// <param name="validator">Validador FluentValidation.</param>
    /// <param name="request">Objeto a ser validado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    protected async Task ValidateAsync<T>(IValidator<T> validator, T request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            Logger.LogWarning("Validação falhou para {RequestType}: {Errors}", 
                typeof(T).Name, string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));
            
            throw new Hephaestus.Application.Exceptions.ValidationException(validationResult);
        }
    }

    /// <summary>
    /// Verifica se um recurso existe e lança NotFoundException se não existir.
    /// </summary>
    /// <typeparam name="T">Tipo do recurso.</typeparam>
    /// <param name="resource">Recurso a ser verificado.</param>
    /// <param name="resourceType">Tipo do recurso para a mensagem de erro.</param>
    /// <param name="resourceId">ID do recurso para a mensagem de erro.</param>
    /// <returns>O recurso se existir.</returns>
    protected T EnsureResourceExists<T>(T? resource, string resourceType, string resourceId)
    {
        if (resource == null)
        {
            Logger.LogWarning("{ResourceType} com ID '{ResourceId}' não encontrado.", resourceType, resourceId);
            throw new NotFoundException(resourceType, resourceId);
        }

        return resource;
    }

    /// <summary>
    /// Verifica se uma entidade existe e lança NotFoundException se não existir.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade.</typeparam>
    /// <param name="entity">Entidade a ser verificada.</param>
    /// <param name="entityType">Tipo da entidade para a mensagem de erro.</param>
    /// <param name="entityId">ID da entidade para a mensagem de erro.</param>
    /// <returns>A entidade se existir.</returns>
    protected T EnsureEntityExists<T>(T? entity, string entityType, string entityId)
    {
        return EnsureResourceExists(entity, entityType, entityId);
    }

    /// <summary>
    /// Verifica se uma operação é permitida e lança UnauthorizedException se não for.
    /// </summary>
    /// <param name="condition">Condição que deve ser verdadeira.</param>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="action">Ação que o usuário tentou realizar.</param>
    /// <param name="resource">Recurso que o usuário tentou acessar.</param>
    protected void EnsureAuthorized(bool condition, string message, string action, string resource)
    {
        if (!condition)
        {
            Logger.LogWarning("Acesso negado: {Message}", message);
            throw new UnauthorizedException(message, action, resource);
        }
    }

    /// <summary>
    /// Verifica se uma operação é permitida e lança UnauthorizedException se não for.
    /// </summary>
    /// <param name="condition">Condição que deve ser verdadeira.</param>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="action">Ação que o usuário tentou realizar.</param>
    /// <param name="resource">Recurso que o usuário tentou acessar.</param>
    protected void ValidateAuthorization(bool condition, string message, string action, string resource)
    {
        EnsureAuthorized(condition, message, action, resource);
    }

    /// <summary>
    /// Verifica se uma regra de negócio é respeitada e lança BusinessRuleException se não for.
    /// </summary>
    /// <param name="condition">Condição que deve ser verdadeira.</param>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="ruleName">Nome da regra de negócio.</param>
    /// <param name="details">Detalhes adicionais sobre o erro.</param>
    protected void EnsureBusinessRule(bool condition, string message, string ruleName, IDictionary<string, object>? details = null)
    {
        if (!condition)
        {
            Logger.LogWarning("Regra de negócio violada: {RuleName} - {Message}", ruleName, message);
            throw new BusinessRuleException(message, ruleName, details);
        }
    }

    /// <summary>
    /// Verifica se uma regra de negócio é respeitada e lança BusinessRuleException se não for.
    /// </summary>
    /// <param name="condition">Condição que deve ser verdadeira.</param>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="ruleName">Nome da regra de negócio.</param>
    /// <param name="details">Detalhes adicionais sobre o erro.</param>
    protected void ValidateBusinessRule(bool condition, string message, string ruleName, IDictionary<string, object>? details = null)
    {
        EnsureBusinessRule(condition, message, ruleName, details);
    }

    /// <summary>
    /// Verifica se não há conflito e lança ConflictException se houver.
    /// </summary>
    /// <param name="condition">Condição que deve ser verdadeira (não há conflito).</param>
    /// <param name="message">Mensagem de erro.</param>
    /// <param name="resourceType">Tipo do recurso em conflito.</param>
    /// <param name="conflictingField">Campo que causou o conflito.</param>
    /// <param name="conflictingValue">Valor que causou o conflito.</param>
    protected void EnsureNoConflict(bool condition, string message, string resourceType, string conflictingField, string conflictingValue)
    {
        if (!condition)
        {
            Logger.LogWarning("Conflito detectado: {ResourceType} - {Field}: {Value}", resourceType, conflictingField, conflictingValue);
            throw new ConflictException(message, resourceType, conflictingField, conflictingValue);
        }
    }

    /// <summary>
    /// Executa uma operação com tratamento de exceções.
    /// </summary>
    /// <typeparam name="T">Tipo do resultado.</typeparam>
    /// <param name="operation">Operação a ser executada.</param>
    /// <param name="operationName">Nome da operação para logging.</param>
    /// <returns>Resultado da operação.</returns>
    protected async Task<T> ExecuteWithExceptionHandlingAsync<T>(Func<Task<T>> operation, string operationName)
    {
        try
        {
            Logger.LogDebug("Executando operação: {OperationName}", operationName);
            var result = await operation();
            Logger.LogDebug("Operação {OperationName} executada com sucesso.", operationName);
            return result;
        }
        catch (Exception ex) when (ExceptionHandler.CanHandle(ex))
        {
            Logger.LogWarning(ex, "Erro tratável na operação {OperationName}: {Message}", operationName, ex.Message);
            // Não relança exceções tratáveis - deixa o middleware capturar
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro inesperado na operação {OperationName}: {Message}", operationName, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Executa uma operação void com tratamento de exceções.
    /// </summary>
    /// <param name="operation">Operação a ser executada.</param>
    /// <param name="operationName">Nome da operação para logging.</param>
    protected async Task ExecuteWithExceptionHandlingAsync(Func<Task> operation, string operationName)
    {
        try
        {
            Logger.LogDebug("Executando operação: {OperationName}", operationName);
            await operation();
            Logger.LogDebug("Operação {OperationName} executada com sucesso.", operationName);
        }
        catch (Exception ex) when (ExceptionHandler.CanHandle(ex))
        {
            Logger.LogWarning(ex, "Erro tratável na operação {OperationName}: {Message}", operationName, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro inesperado na operação {OperationName}: {Message}", operationName, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Executa uma operação com tratamento de exceções (sobrecarga simplificada).
    /// </summary>
    /// <typeparam name="T">Tipo do resultado.</typeparam>
    /// <param name="operation">Operação a ser executada.</param>
    /// <returns>Resultado da operação.</returns>
    protected async Task<T> ExecuteWithExceptionHandlingAsync<T>(Func<Task<T>> operation)
    {
        return await ExecuteWithExceptionHandlingAsync(operation, "Operação");
    }

    /// <summary>
    /// Executa uma operação void com tratamento de exceções (sobrecarga simplificada).
    /// </summary>
    /// <param name="operation">Operação a ser executada.</param>
    protected async Task ExecuteWithExceptionHandlingAsync(Func<Task> operation)
    {
        await ExecuteWithExceptionHandlingAsync(operation, "Operação");
    }
} 