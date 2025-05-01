namespace Backend.TCC.PUCRS.Model;
public class RefreshTokenModel
{
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsRevoked { get; set; } = false;
}