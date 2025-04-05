using System.Security.Cryptography;
using System.Text;
namespace Backend.TCC.PUCRS.Utils;
public class Utils
{
    public string GeneratePassword(int length)
    {
        if (length < 8) { length = 8; }

        const string upperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
        const string numbers = "0123456789";
        const string specialCharacters = "!@#$%^&*()-_=+[]{}|;:',.<>?/";

        // Garantir que a senha contenha pelo menos um caractere de cada grupo
        var random = new Random();
        var password = new StringBuilder();

        password.Append(upperCaseLetters[random.Next(upperCaseLetters.Length)]);
        password.Append(lowerCaseLetters[random.Next(lowerCaseLetters.Length)]);
        password.Append(numbers[random.Next(numbers.Length)]);
        password.Append(specialCharacters[random.Next(specialCharacters.Length)]);

        // Adicionar caracteres aleatórios para atingir o comprimento desejado
        string allCharacters = upperCaseLetters + lowerCaseLetters + numbers + specialCharacters;
        for (int i = password.Length; i < length; i++)
        {
            password.Append(allCharacters[random.Next(allCharacters.Length)]);
        }

        // Embaralhar os caracteres da senha para maior aleatoriedade
        return new string(password.ToString().OrderBy(_ => random.Next()).ToArray());
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

    public string GerarHash(string senha)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(senha);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
    public bool VerificarSenha(string senha, string hashArmazenado)
    {
        string hashDaSenha = GerarHash(senha);
        return hashDaSenha == hashArmazenado;
    }
}