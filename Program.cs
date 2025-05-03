using System.Text;
using Backend.TCC.PUCRS.Services;
using Backend.TCC.PUCRS.Services.Interfaces;
using Backend.TCC.PUCRS.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") // Altere para a origem do seu frontend
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Configuração de logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddSingleton<DatabaseHelper>();

//Configuracao de versionamento
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true; // Relatar versões da API
    options.AssumeDefaultVersionWhenUnspecified = true; // Assumir versão padrão se não especificada
    options.DefaultApiVersion = new ApiVersion(1, 0); // Definir versão padrão
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Formato do nome do grupo
    options.SubstituteApiVersionInUrl = true; // Substituir versão na URL
});

// Adicionar serviços ao contêiner
builder.Services.AddControllers();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ILogGeral, LogGeralService>();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<Utilidades>();

// Configurações para banco de dados e token jwt
Configuracao.CarregarConfiguracoes(builder);



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Validar emissor
            ValidIssuer = Configuracao.JwtIssuer, // Emissor esperado
            ValidateAudience = true, // Validar público
            ValidAudience = Configuracao.JwtAudience, // Público esperado
            ValidateLifetime = true, // Validar tempo de expiração
            ValidateIssuerSigningKey = true, // Validar assinatura
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuracao.JwtSecretKey)),
            ClockSkew = TimeSpan.Zero // Sem tolerância para expiração
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
});

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Minha API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT como: Bearer {seu token}",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    });
});



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minha API v1");
        c.RoutePrefix = string.Empty; // opcional, mostra Swagger na raiz (localhost:5000/)
    });
}


// Aplicar o CORS
app.UseCors("AllowFrontend");

// Configuração do pipeline de middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
