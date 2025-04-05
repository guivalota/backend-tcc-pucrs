public interface IPessoaService
{
    Task<Pessoa?> GetPessoaByLoginAsync(string login);
    Task<Pessoa?> AddPessoa(Pessoa pessoa);
    Task UpdatePessoa(Pessoa pessoa);
}