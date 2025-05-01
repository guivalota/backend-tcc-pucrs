
using Backend.TCC.PUCRS.Model;
using Backend.TCC.PUCRS.Services;
using Backend.TCC.PUCRS.Services.Interfaces;
using Microsoft.Data.SqlClient;

public class RoleService : IRoleService
{
    private readonly DatabaseHelper _dbHelper;
    private readonly ILogGeral _logGeral;
    private readonly AuthService _authService;

    public RoleService(DatabaseHelper dbHelper, ILogGeral logGeral, AuthService authService)
    {
        _dbHelper = dbHelper;
        _logGeral = logGeral;
        _authService = authService;
    }
    public async void AddRole(Role role)
    {
        var insertQuery = "INSERT INTO Role (Descricao) VALUES (@Descricao)";
        var insertParams = new[]
        {
                    new SqlParameter("@Descricao", role.Descricao)
                };
        await _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);
        _logGeral.AddLogGeral(CreateLog($"Role {role.Descricao} foi adicionada", "Role", 0));
    }

    public async void AlterRole(Role role)
    {
        if (role.Id == 1)
        {
            throw new UnauthorizedAccessException("Não é possível alterar a role Admin.");
        }
        if (role.Id == 2)
        {
            throw new UnauthorizedAccessException("Não é possível alterar a role User.");
        }

        var updateQuery = "UPDATE Role set Descricao = @Descricao where Id = @Id";
        var updateParams = new[]
        {
            new SqlParameter("@Id", role.Id),
            new SqlParameter("@Descricao", role.Descricao)
        };
        await _dbHelper.ExecuteCommandAsync(updateQuery, updateParams);
        _logGeral.AddLogGeral(CreateLog($"Role {role.Descricao} foi alterada", "Role", 0));
    }

    public async void DeleteRole(Role role)
    {
        if (role.Id == 1)
        {
            throw new UnauthorizedAccessException("Não é possível excluir a role Admin.");
        }
        if (role.Id == 2)
        {
            throw new UnauthorizedAccessException("Não é possível excluir a role User.");
        }

        var deleteQuery = "DELETE FROM Role where Id = @Id";
        var deleteParams = new[]
        {
            new SqlParameter("@Id", role.Id)
        };
        await _dbHelper.ExecuteCommandAsync(deleteQuery, deleteParams);
        _logGeral.AddLogGeral(CreateLog($"Role {role.Descricao} foi excluida.", "Role", 0));
    }

    public async Task<Role?> GetRoleById(int id)
    {
        var query = "SELECT Id, Descricao from Role where Id = @Id ";
        var parameters = new[]
        {
                new SqlParameter("@Id", id)
            };
        using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
        if (reader.Read())
        {
            return new Role
            {
                Id = reader.GetInt32(0),
                Descricao = reader.GetString(1)
            };
        }
        return null;
    }

    public async Task<Role?> GetRoleByDescricao(string descricao)
    {
        var query = "SELECT Id, Descricao from Role where Descricao = @Descricao ";
        var parameters = new[]
        {
                new SqlParameter("@Descricao", descricao)
            };
        using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
        if (reader.Read())
        {
            return new Role
            {
                Id = reader.GetInt32(0),
                Descricao = reader.GetString(1)
            };
        }
        return null;
    }

    public async Task<List<Role>> ListRoles()
    {
        var retorno = new List<Role>();
        var query = " SELECT Id, Descricao From Role";
        using var reader = await _dbHelper.ExecuteReaderAsync(query);
        while (reader.Read())
        {
            retorno.Add(new Role
            {
                Id = reader.GetInt32(0),
                Descricao = reader.GetString(1)
            });
        }
        return retorno;
    }

    public LogGeral CreateLog(string message, string tabela, int idUsuario)
    {
        LogGeral logGeralModel = new LogGeral();
        logGeralModel.Message = message;
        logGeralModel.Table = tabela;
        logGeralModel.IdUsuario = idUsuario;
        logGeralModel.Data = DateTime.Now;

        return logGeralModel;
    }
}
