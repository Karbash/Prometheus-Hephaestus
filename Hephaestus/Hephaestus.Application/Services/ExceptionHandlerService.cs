using FluentValidation;
using Hephaestus.Application.Exceptions;
using Microsoft.Extensions.Logging;
using SystemApplicationException = System.ApplicationException;
using FluentValidationException = FluentValidation.ValidationException;

namespace Hephaestus.Application.Services;

/// <summary>
/// Implementação do serviço de tratamento centralizado de exceções.
/// </summary>
public class ExceptionHandlerService : IExceptionHandlerService
{
    private readonly ILogger<ExceptionHandlerService> _logger;

    public ExceptionHandlerService(ILogger<ExceptionHandlerService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Trata uma exceção e retorna informações estruturadas sobre o erro.
    /// </summary>
    /// <param name="exception">Exceção a ser tratada.</param>
    /// <returns>Informações estruturadas sobre o erro.</returns>
    public ExceptionInfo HandleException(Exception exception)
    {
        _logger.LogDebug("Tratando exceção do tipo: {ExceptionType}", exception.GetType().Name);

        return exception switch
        {
            FluentValidationException validationEx => HandleValidationException(validationEx),
            Hephaestus.Application.Exceptions.ValidationException customValidationEx => HandleCustomValidationException(customValidationEx),
            NotFoundException notFoundEx => HandleNotFoundException(notFoundEx),
            BusinessRuleException businessRuleEx => HandleBusinessRuleException(businessRuleEx),
            UnauthorizedException unauthorizedEx => HandleUnauthorizedException(unauthorizedEx),
            ConflictException conflictEx => HandleConflictException(conflictEx),
            SystemApplicationException appEx => HandleApplicationException(appEx),
            ArgumentException argEx => HandleArgumentException(argEx),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),
            KeyNotFoundException keyNotFoundEx => HandleKeyNotFoundException(keyNotFoundEx),
            _ => HandleUnexpectedException(exception)
        };
    }

    /// <summary>
    /// Verifica se uma exceção é tratável pelo serviço.
    /// </summary>
    /// <param name="exception">Exceção a ser verificada.</param>
    /// <returns>True se a exceção pode ser tratada.</returns>
    public bool CanHandle(Exception exception)
    {
        return exception is FluentValidationException
            || exception is Hephaestus.Application.Exceptions.ValidationException
            || exception is NotFoundException
            || exception is BusinessRuleException
            || exception is UnauthorizedException
            || exception is ConflictException
            || exception is SystemApplicationException
            || exception is ArgumentException
            || exception is InvalidOperationException
            || exception is KeyNotFoundException;
    }

    private ExceptionInfo HandleValidationException(FluentValidationException exception)
    {
        var errors = exception.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }).ToList();
        
        return new ExceptionInfo
        {
            ErrorCode = "VALIDATION_ERROR",
            Message = "Erro de validação",
            StatusCode = 400,
            ErrorType = "ValidationError",
            Details = new Dictionary<string, object>
            {
                ["errors"] = errors
            }
        };
    }

    private ExceptionInfo HandleCustomValidationException(Hephaestus.Application.Exceptions.ValidationException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = "VALIDATION_ERROR",
            Message = exception.Message,
            StatusCode = 400,
            ErrorType = "ValidationError",
            Details = exception.Details
        };
    }

    private ExceptionInfo HandleNotFoundException(NotFoundException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = exception.ErrorCode,
            Message = exception.Message,
            StatusCode = 404,
            ErrorType = "NotFoundError",
            Details = new Dictionary<string, object>
            {
                ["resourceType"] = exception.ResourceType,
                ["resourceId"] = exception.ResourceId
            }
        };
    }

    private ExceptionInfo HandleBusinessRuleException(BusinessRuleException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = exception.ErrorCode,
            Message = exception.Message,
            StatusCode = 400,
            ErrorType = "BusinessRuleError",
            Details = new Dictionary<string, object>
            {
                ["ruleName"] = exception.RuleName
            }.Concat(exception.Details ?? new Dictionary<string, object>()).ToDictionary(x => x.Key, x => x.Value)
        };
    }

    private ExceptionInfo HandleUnauthorizedException(UnauthorizedException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = exception.ErrorCode,
            Message = exception.Message,
            StatusCode = 403,
            ErrorType = "UnauthorizedError",
            Details = new Dictionary<string, object>
            {
                ["action"] = exception.Action,
                ["resource"] = exception.Resource
            }
        };
    }

    private ExceptionInfo HandleConflictException(ConflictException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = exception.ErrorCode,
            Message = exception.Message,
            StatusCode = 409,
            ErrorType = "ConflictError",
            Details = new Dictionary<string, object>
            {
                ["resourceType"] = exception.ResourceType,
                ["conflictingField"] = exception.ConflictingField,
                ["conflictingValue"] = exception.ConflictingValue
            }
        };
    }

    private ExceptionInfo HandleApplicationException(SystemApplicationException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = "APPLICATION_ERROR",
            Message = exception.Message,
            StatusCode = 400,
            ErrorType = "ApplicationError"
        };
    }

    private ExceptionInfo HandleArgumentException(ArgumentException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = "INVALID_ARGUMENT",
            Message = exception.Message,
            StatusCode = 400,
            ErrorType = "ArgumentError",
            Details = new Dictionary<string, object>
            {
                ["paramName"] = exception.ParamName ?? "unknown"
            }
        };
    }

    private ExceptionInfo HandleInvalidOperationException(InvalidOperationException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = "INVALID_OPERATION",
            Message = exception.Message,
            StatusCode = 400,
            ErrorType = "InvalidOperationError"
        };
    }

    private ExceptionInfo HandleKeyNotFoundException(KeyNotFoundException exception)
    {
        return new ExceptionInfo
        {
            ErrorCode = "RESOURCE_NOT_FOUND",
            Message = exception.Message,
            StatusCode = 404,
            ErrorType = "NotFoundError"
        };
    }

    private ExceptionInfo HandleUnexpectedException(Exception exception)
    {
        _logger.LogError(exception, "Exceção inesperada: {Message}", exception.Message);

        return new ExceptionInfo
        {
            ErrorCode = "INTERNAL_SERVER_ERROR",
            Message = "Erro interno do servidor",
            StatusCode = 500,
            ErrorType = "UnexpectedError"
        };
    }
} 