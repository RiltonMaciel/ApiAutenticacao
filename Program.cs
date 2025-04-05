using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ApiAutenticacao.Data;
using ApiAutenticacao.Services;
using ApiAutenticacao.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 🔹 1. Adiciona os controllers da API
builder.Services.AddControllers();


// 🔹 2. Configuração do Swagger (documentação da API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Autenticação",
        Version = "v1"
    });

    // 🛡️ Adiciona ao Swagger a opção de inserir o token JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT: Bearer {seu token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// 🔹 3. Conexão com o banco de dados SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// 🔹 4. Injeta o serviço de autenticação (injeção de dependência)
builder.Services.AddScoped<IAuthService, AuthService>();


// ✅ 5. 🔐 Configuração do JWT

// 🔎 Pega as configurações do token do appsettings.json
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

// 🛡️ Adiciona a autenticação com esquema JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Se quiser validar o emissor, mude para true
        ValidateAudience = false, // Se quiser validar o público, mude para true
        ValidateLifetime = true, // Expiração do token
        ValidateIssuerSigningKey = true, // Valida a chave secreta
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});


var app = builder.Build();


// 🔹 6. Aplica as migrations automaticamente ao iniciar o app
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}


// 🔹 7. Configura o Swagger para aparecer direto na raiz da API
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Autenticação V1");
        c.RoutePrefix = string.Empty;
    });
}


// 🔹 8. Middlewares HTTP
app.UseHttpsRedirection();

app.UseAuthentication(); // ✅ Importante: ativa a verificação do token antes de tudo
app.UseAuthorization();  // ✅ Verifica se o usuário tem permissão para acessar

app.MapControllers(); // 🔹 Mapeia os endpoints do controller automaticamente

app.Run(); // 🔚 Inicia a aplicação
