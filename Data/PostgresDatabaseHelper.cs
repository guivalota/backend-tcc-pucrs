using System.Data;
using Npgsql;

public class PostgresDatabaseHelper
{
    private readonly string _connectionString;

    public PostgresDatabaseHelper(IConfiguration configuration)
    {
        _connectionString = Configuracao.PostgresConnectionString;
    }

    public NpgsqlConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public async Task<int> ExecuteCommandAsync(string query, Dictionary<string, object?>? parameters = null)
    {
        using var connection = GetConnection();
        using var command = new NpgsqlCommand(query, connection);

        if (parameters != null)
        {
            foreach (var kvp in parameters)
            {
                var param = DbParameterHelper.CreateParameter(kvp.Key, kvp.Value);
                command.Parameters.Add(param);
            }
        }

        await connection.OpenAsync();
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<NpgsqlDataReader> ExecuteReaderAsync(string query, Dictionary<string, object?>? parameters = null)
    {
        try
        {
            var connection = GetConnection();
            var command = new NpgsqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    var param = DbParameterHelper.CreateParameter(kvp.Key, kvp.Value);
                    command.Parameters.Add(param);
                }
            }

            await connection.OpenAsync();
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
        catch (Exception ex)
        {
            throw new Exception("Erro ao executar o comando no PostgreSQL: " + ex.Message, ex);
        }
    }
}