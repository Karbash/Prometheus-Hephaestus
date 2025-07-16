namespace Hephaestus.Domain.DTOs.Response
{
    public class ExecuteQueryResponse
    {
        public List<Dictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
    }
}
