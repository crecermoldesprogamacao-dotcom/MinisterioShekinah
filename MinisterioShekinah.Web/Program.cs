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

/// 2. Configuração direta do banco de dados PostgreSQL do Render (Formato Chave=Valor)
var connectionString = "Host=dpg-dbm7d6l6e07s73bcmjq0-a;Database=ministerio_shekinah_db;Username=ministerio_shekinah_db_user;Password=M6mFlNXNtA8H6LanRGOH9UsmL94N2EcB;Port=5432;SSL Mode=Prefer;Trust Server Certificate=true";

builder.Services.AddDbContext<ShekinahContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Bloco de verificação e criação automática do banco de dados
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ShekinahContext>();

        context.Database.EnsureCreated();
        Console.WriteLine("--> Banco de dados PostgreSQL verificado e criado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> AVISO NA CRIAÇÃO DO BANCO POSTGRESQL: {ex.Message}");
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