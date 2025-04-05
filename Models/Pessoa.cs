using System.Text.Json.Serialization;

public class Pessoa
{
    [JsonPropertyName("Id")]
    public int Id { get; set; }
    [JsonPropertyName("user")]
    public Users? user { get; set; }
    [JsonPropertyName("Nome")]
    public string? Nome { get; set; }
    [JsonPropertyName("Sobrenome")]
    public string? Sobrenome { get; set; }
    [JsonPropertyName("Documento")]
    public string? Documento { get; set; }
}
