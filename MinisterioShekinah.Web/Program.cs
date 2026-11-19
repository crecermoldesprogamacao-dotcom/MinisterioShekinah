using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Hubs;
using MinisterioShekinah.Web.Models;
using MinisterioShekinah.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Adicionar os serviços necessários ao container do ASP.NET Core
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<CifraAiService>();
builder.Services.AddHttpClient<LiturgiaService>();
builder.Services.AddScoped<IAEngineService>();
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

// 2. Configuração do banco de dados local SQLite com DbContext
builder.Services.AddDbContext<ShekinahContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Bloco de criação automática do banco SQLite local
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ShekinahContext>();
        // Cria o arquivo ministerioshekinah.db e todas as tabelas se ainda não existirem
        context.Database.EnsureCreated();
        Console.WriteLine("--> Banco de dados SQLite local verificado e criado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> AVISO NA CRIAÇÃO DO BANCO LOCAL: {ex.Message}");
    }
}

// 3. Configuração do pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

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

// Configuração da porta dinâmica para o Render e ambiente de produção
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");