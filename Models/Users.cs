using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Users
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    [JsonPropertyName("Email")]
    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um endereço de email válido.")]
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    [Required(ErrorMessage = "O campo Login é obrigatório.")]
    [JsonPropertyName("Login")]
    public string Login { get; set; } = "";
    public string EmailVerificationToken { get; set; } = "";
    public bool EmailVerified { get; set; }
    public List<Role> roles = new List<Role>();
}