using System.Text;
using Backend.TCC.PUCRS.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:7296") // Altere para a origem do seu frontend
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Configuração de logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddSingleton<DatabaseHelper>();

// Adicionar serviços ao contêiner
builder.Services.AddControllers();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ILogGeral, LogGeralService>();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<Utils>();

// Configurações para validação JWT
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"];
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Validar emissor
            ValidIssuer = jwtIssuer, // Emissor esperado
            ValidateAudience = true, // Validar público
            ValidAudience = jwtAudience, // Público esperado
            ValidateLifetime = true, // Validar tempo de expiração
            ValidateIssuerSigningKey = true, // Validar assinatura
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey!)),
            ClockSkew = TimeSpan.Zero // Sem tolerância para expiração
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Aplicar o CORS
app.UseCors("AllowFrontend");

// Configuração do pipeline de middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
