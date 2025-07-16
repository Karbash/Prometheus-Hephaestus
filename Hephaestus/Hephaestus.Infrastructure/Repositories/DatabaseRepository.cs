using Hephaestus.Domain.Repositories;
using Hephaestus.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Hephaestus.Infrastructure.Repositories;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly HephaestusDbContext _dbContext;
    private readonly ILogger<DatabaseRepository> _logger;

    public DatabaseRepository(HephaestusDbContext dbContext, ILogger<DatabaseRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query é obrigatória.", nameof(query));

        // Validação básica para evitar comandos perigosos
        if (query.ToLower().Contains("delete") ||
            query.ToLower().Contains("update") ||
            query.ToLower().Contains("drop") ||
            query.ToLower().Contains("insert"))
            throw new ArgumentException("Apenas consultas SELECT são permitidas.");

        var results = new List<Dictionary<string, object>>();

        try
        {
            using var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }

            _logger.LogInformation("Query executada com sucesso: {Query}", query);
            return results;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "42P01")
        {
            var schemas = await GetAvailableSchemasAsync();
            var tableName = ExtractTableName(query);
            _logger.LogError(ex, "Tabela '{TableName}' não encontrada na query: {Query}. Schemas disponíveis: {Schemas}. Tente usar aspas para nomes com maiúsculas (ex.: \"Tags\").", tableName, query, string.Join(", ", schemas));
            throw new ArgumentException($"Tabela '{tableName}' não existe. Schemas disponíveis: {string.Join(", ", schemas)}. Tente usar aspas para nomes com maiúsculas (ex.: \"Tags\").", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar query: {Query}", query);
            throw new Exception($"Erro ao executar a query: {ex.Message}", ex);
        }
    }

    private async Task<List<string>> GetAvailableSchemasAsync()
    {
        var schemas = new List<string>();
        try
        {
            using var connection = _dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog', 'information_schema')";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                schemas.Add(reader.GetString(0));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter schemas disponíveis.");
        }
        return schemas;
    }

    private string ExtractTableName(string query)
    {
        var parts = query.ToLower().Split(new[] { "from" }, StringSplitOptions.None);
        if (parts.Length > 1)
        {
            var tablePart = parts[1].Trim().Split(' ')[0];
            return tablePart.Replace("\"", "");
        }
        return "desconhecida";
    }
}
