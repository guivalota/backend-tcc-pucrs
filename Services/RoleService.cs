
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
    public void AddRole(Role role)
    {
        throw new NotImplementedException();
    }

    public void AlterRole(Role role)
    {
        throw new NotImplementedException();
    }

    public LogGeral CreateLog(string message, string tabela, int idUsuario)
    {
        throw new NotImplementedException();
    }

    public void DeleteRole(Role role)
    {
        throw new NotImplementedException();
    }

    public Task<Role?> GetRoleByDescricao(string descricao)
    {
        throw new NotImplementedException();
    }

    public Task<Role?> GetRoleById(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Role>> ListRoles()
    {
        throw new NotImplementedException();
    }
}
