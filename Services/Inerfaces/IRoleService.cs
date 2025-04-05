public interface IRoleService
{
    void AddRole(Role role);
    Task<List<Role>> ListRoles();
    void AlterRole(Role role);
    void DeleteRole(Role role);
    Task<Role?> GetRoleByDescricao(string descricao);
    Task<Role?> GetRoleById(int id);
    LogGeral CreateLog(string message, string tabela, int idUsuario);
}