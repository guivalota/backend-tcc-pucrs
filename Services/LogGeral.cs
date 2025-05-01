using Backend.TCC.PUCRS.Model;
using Backend.TCC.PUCRS.Services.Interfaces;
using Microsoft.Data.SqlClient;

public class LogGeralService : ILogGeral
{
    private readonly DatabaseHelper _dbHelper;

    public LogGeralService(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }
    public void AddLogGeral(LogGeral log)
    {
        try
        {
            var insertQuery = "INSERT INTO LogGeral (message, table_name, data, IdUsuario) VALUES (@Message, @Table, GetDate(), @IdUsuario)";
            var insertParams = new[]
            {
                    new SqlParameter("@Message", log.Message),
                    new SqlParameter("@Table", log.Table),
                    new SqlParameter("@IdUsuario", log.IdUsuario)
                };
            var _ = _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro:{ex.Message}");
        }
    }
}