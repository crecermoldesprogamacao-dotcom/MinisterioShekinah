using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Hubs;
using MinisterioShekinah.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Adicionar os serviços necessários ao container do ASP.NET Core
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization();

// Adicionar serviço em tempo real do SignalR
builder.Services.AddSignalR();

// Adicionar serviços de Sessão para o Painel Administrativo
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuração inteligente do Banco de Dados:
// Se estiver no Render, ele lê a variável de ambiente; se estiver no seu PC, usa o appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Configuração do banco de dados PostgreSQL com DbContext
builder.Services.AddDbContext<ShekinahContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Bloco para garantir que a base de dados e as tabelas existem na nuvem
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ShekinahContext>();

    // Para PostgreSQL no Render, o Migrate() garante a criação correta das tabelas
    context.Database.Migrate();
}

// 3. Configuração do pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Ativar o middleware de Sessão
app.UseSession();
app.UseAuthorization();

// Rota padrão do MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Endpoint em tempo real para as Velas Virtuais
app.MapHub<VelaHub>("/velaHub");

app.Run();