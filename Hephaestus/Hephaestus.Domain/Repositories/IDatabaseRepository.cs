namespace Hephaestus.Domain.Repositories
{
    public interface IDatabaseRepository
    {
        Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query);
    }
}
