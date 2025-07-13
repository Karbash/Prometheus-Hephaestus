using System.Threading.Tasks;

namespace Hephaestus.Application.Interfaces.Promotion;

public interface IDeletePromotionUseCase
{
    Task ExecuteAsync(string id, string tenantId);
}