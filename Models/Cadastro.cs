namespace Backend.TCC.PUCRS.Model;
public class Cadastro
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Login { get; set; }
    public string? EmailVerificationToken { get; set; }
    public bool EmailVerified { get; set; }
}