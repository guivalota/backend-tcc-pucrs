public static class Configuracao
{
    public static string ConnectionString { get; set; } = string.Empty;
    public static string SqlServer { get; set; } = string.Empty;
    public static string SqlPort { get; set; } = string.Empty;
    public static string SqlUser { get; set; } = string.Empty;
    public static string SqlPassword { get; set; } = string.Empty;
    public static string SqlDataBase { get; set; } = string.Empty;

    public static string JwtSecretKey { get; set; } = string.Empty;
    public static string JwtIssuer { get; set; } = string.Empty;
    public static string JwtAudience { get; set; } = string.Empty;
    public static string SecretKey { get; set; } = string.Empty;
    public static int ExpirationTimeInMinutes { get; set; } = 0;

    public static void CarregarConfiguracoes(WebApplicationBuilder builder)
    {
        JwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]!;
        JwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
        JwtAudience = builder.Configuration["JwtSettings:Audience"]!;

        SqlPort = builder.Configuration["SqlSettings:Port"]!;
        SqlUser = builder.Configuration["SqlSettings:User"]!;
        SqlPassword = builder.Configuration["SqlSettings:Password"]!;
        SqlServer = builder.Configuration["SqlSettings:Server"]!;
        SqlDataBase = builder.Configuration["SqlSettings:Database"]!;
        ConnectionString = $"Server={SqlServer},{SqlPort};Database={SqlDataBase};User Id={SqlUser};Password={SqlPassword};TrustServerCertificate=True;Encrypt=False;Connection Timeout=30;";
    }
}
