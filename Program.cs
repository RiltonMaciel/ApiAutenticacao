using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ApiAutenticacao.Data;
using ApiAutenticacao.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o serviço de controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Autenticação",
        Version = "v1"
    });
});

// Conexão com SQL Server (autenticação do Windows)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeta o serviço de autenticação
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Aplica a migration automaticamente ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Ativa Swagger em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Autenticação V1");
        c.RoutePrefix = string.Empty; // Faz com que o Swagger abra direto na raiz
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
