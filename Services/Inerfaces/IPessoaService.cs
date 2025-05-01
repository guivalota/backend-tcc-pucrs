using Backend.TCC.PUCRS.Model;

namespace Backend.TCC.PUCRS.Services.Interfaces;
public interface IPessoaService
{
    Task<Pessoa?> GetPessoaByLoginAsync(string login);
    Task<Pessoa?> AddPessoa(Pessoa pessoa);
    Task UpdatePessoa(Pessoa pessoa);
}