
using Backend.TCC.PUCRS.Model;
using Backend.TCC.PUCRS.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Backend.TCC.PUCRS.Utils;
using Backend.TCC.PUCRS.Services;

public class PessoaService : IPessoaService
{
    private readonly DatabaseHelper _dbHelper;
    private readonly ILogGeral _logGeral;
    private readonly AuthService _authService;
    private readonly Utilidades _utils;

    public PessoaService(DatabaseHelper dbHelper, ILogGeral logGeral, AuthService authService, Utilidades utils)
    {
        _dbHelper = dbHelper;
        _logGeral = logGeral;
        _authService = authService;
        _utils = utils;
    }

    public async Task<Pessoa?> AddPessoa(Pessoa pessoa)
    {
        var insertQuery = "INSERT INTO Pessoa (IdUsuario, nome, sobrenome, documento) VALUES (@IdUsuario, @Nome, @Sobrenome, @Documento)";
        var insertParams = new Dictionary<string, object?>
        {
                    {"@IdUsuario", pessoa.user!.Id },
                    {"@Nome", pessoa!.Nome },
                    {"@Sobrenome", pessoa!.Sobrenome },
                    {"@Documento", pessoa!.Documento }
                };
        await _dbHelper.ExecuteCommandAsync(insertQuery, insertParams);
        return await GetPessoaByLoginAsync(pessoa.user.Login);
    }

    public async Task<Pessoa?> GetPessoaByLoginAsync(string login)
    {
        Users? user = await _authService.GetUserByUserNameAsync(login);
        if (user != null)
        {
            var query = "SELECT Id, nome, sobrenome, documento from Pessoa where IdUsuario = @IdUsuario ";
            var parameters = new Dictionary<string, object?>
            {
                {"@IdUsuario", user.Id }
            };
            using var reader = await _dbHelper.ExecuteReaderAsync(query, parameters);
            if (reader.Read())
            {
                _logGeral.AddLogGeral(_utils.CreateLog($"Retornado pessoa do usuário {login} .", "Pessoa", 0));
                return new Pessoa
                {
                    Id = reader.GetInt32(0),
                    user = user,
                    Nome = reader.GetString(1),
                    Sobrenome = reader.GetString(2),
                    Documento = reader.GetString(3)
                };
            }
            _logGeral.AddLogGeral(_utils.CreateLog($"Pessoa do usuário {login} não localizado.", "Pessoa", 0));
            Pessoa newPessoa = new Pessoa
            {
                user = user,
                Nome = "",
                Sobrenome = "",
                Documento = ""
            };
            return await AddPessoa(newPessoa);
        }
        return null;
    }

    public async Task UpdatePessoa(Pessoa pessoa)
    {
        try
        {
            Users? user = await _authService.GetUserByUserNameAsync(pessoa.user!.Login);
            if (user != null)
            {
                var query = "UPDATE Pessoa set nome = @Nome, sobrenome = @Sobrenome, documento = @Documento where Id = @Id and IdUsuario = @IdUsuario ";
                var parameters = new Dictionary<string, object?>
                {
                {"@Nome", pessoa.Nome },
                {"@Sobrenome", pessoa.Sobrenome },
                {"@Documento", pessoa.Documento },
                {"@Id", pessoa.Id },
                {"@IdUsuario", user.Id }
            };
                await _dbHelper.ExecuteCommandAsync(query, parameters);
                _logGeral.AddLogGeral(_utils.CreateLog($"Usuário {pessoa.user.Login} atualizou as informações pessoais.", "Pessoa", 0));
            }
            else
            {
                _logGeral.AddLogGeral(_utils.CreateLog($"Usuário {pessoa.user.Login} não localizado para efetuar a atualização.", "Pessoa", 0));
                throw new Exception($"Usuário {pessoa.user.Login} não localizado para efetuar a atualização.");
            }
        }
        catch (Exception ex)
        {
            _logGeral.AddLogGeral(_utils.CreateLog($"Ocorreu um erro ao atualizar as informações pessoas do usuário {pessoa.user!.Login}. Erro{ex.Message}", "Pessoa", 0));
            throw new Exception(ex.Message);
        }
    }
}
