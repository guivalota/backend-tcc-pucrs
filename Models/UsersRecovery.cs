namespace Backend.TCC.PUCRS.Model;
public class UsersRecovery
{
    public int Id { get; set; }
    public int IdUser { get; set; }
    public string VerificationToken { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime ExpiryDate { get; set; }
}