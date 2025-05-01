using System.Data;
using Microsoft.Data.SqlClient;

public class DatabaseHelper
{
    private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = Configuracao.ConnectionString; //configuration.GetConnectionString("DefaultConnection")!;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<int> ExecuteCommandAsync(string query, SqlParameter[]? parameters = null)
        {
            using var connection = GetConnection();
            using var command = new SqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            await connection.OpenAsync();
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<SqlDataReader> ExecuteReaderAsync(string query, SqlParameter[]? parameters = null)
        {
            var connection = GetConnection();
            var command = new SqlCommand(query, connection);
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            await connection.OpenAsync();
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
}