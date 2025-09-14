using System.Data;
using Microsoft.Data.SqlClient;

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(IConfiguration configuration)
    {
        _connectionString = Configuracao.SqlConnectionString; //configuration.GetConnectionString("DefaultConnection")!;
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }


    public async Task<int> ExecuteCommandAsync(string query, Dictionary<string, object?>? parameters = null)
    {
        using var connection = GetConnection();
        using var command = new SqlCommand(query, connection);

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

    public async Task<SqlDataReader> ExecuteReaderAsync(string query, Dictionary<string, object?>? parameters = null)
    {
        var connection = GetConnection();
        var command = new SqlCommand(query, connection);

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
}