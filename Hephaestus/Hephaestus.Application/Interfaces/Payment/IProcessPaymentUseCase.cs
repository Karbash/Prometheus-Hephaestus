using Hephaestus.Domain.DTOs.Request;
using Hephaestus.Domain.DTOs.Response;
using System.Security.Claims;

namespace Hephaestus.Application.Interfaces.Payment;

public interface IProcessPaymentUseCase
{
    Task<PaymentResponse> ExecuteAsync(PaymentRequest request, ClaimsPrincipal user);
}