using System.Data;
using Npgsql;

public class DbParameterHelper
{
    public static Func<string, object?, IDbDataParameter> CreateParameter = 
        (name, value) => new NpgsqlParameter(name, value ?? DBNull.Value);
}
