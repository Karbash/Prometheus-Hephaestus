using Hephaestus.Domain.Repositories;
using Hephaestus.Application.Interfaces.Coupon;
using Hephaestus.Application.Base;
using Hephaestus.Application.Exceptions;
using Hephaestus.Application.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Hephaestus.Application.UseCases.Coupon;

/// <summary>
/// Caso de uso para remoção de cupons.
/// </summary>
public class DeleteCouponUseCase : BaseUseCase, IDeleteCouponUseCase
{
    private readonly ICouponRepository _couponRepository;

    public DeleteCouponUseCase(
        ICouponRepository couponRepository,
        ILogger<DeleteCouponUseCase> logger,
        IExceptionHandlerService exceptionHandler)
        : base(logger, exceptionHandler)
    {
        _couponRepository = couponRepository;
    }

    public async Task ExecuteAsync(string id, string tenantId)
    {
        await ExecuteWithExceptionHandlingAsync(async () =>
        {
            var coupon = await _couponRepository.GetByIdAsync(id, tenantId);
            EnsureResourceExists(coupon, "Coupon", id);

            await _couponRepository.DeleteAsync(id, tenantId);
        }, "DeleteCoupon");
    }
}