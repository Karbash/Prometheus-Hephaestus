using FluentValidation;
using Hephaestus.Application.Base;
using Hephaestus.Application.Interfaces.Payment;
using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using Hephaestus.Application.Services;
using Hephaestus.Domain.Entities;
using Hephaestus.Domain.Enum;
using Hephaestus.Domain.Repositories;
using Hephaestus.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hephaestus.Application.UseCases;

public class ProcessPaymentUseCase : IProcessPaymentUseCase
{
    private readonly IValidator<PaymentRequest> _validator;
    public ProcessPaymentUseCase(IValidator<PaymentRequest> validator)
    {
        _validator = validator;
    }

    public async Task<PaymentResponse> ExecuteAsync(PaymentRequest request, ClaimsPrincipal user)
    {
        await _validator.ValidateAndThrowAsync(request);
        var now = DateTime.UtcNow;
        return new PaymentResponse
        {
            PaymentId = Guid.NewGuid().ToString(),
            Amount = request.Amount,
            Currency = request.Currency,
            Status = "pending",
            PaymentMethod = request.PaymentMethod,
            CustomerId = request.CustomerId,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}