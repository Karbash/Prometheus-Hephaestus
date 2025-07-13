namespace Hephaestus.Application.Interfaces.Additional
{
    public interface IDeleteAdditionalUseCase
    {
        Task ExecuteAsync(string id, string tenantId);
    }
}
