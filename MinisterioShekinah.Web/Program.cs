using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Hubs;
using MinisterioShekinah.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar a porta dinâmica para o Docker / Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

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

// Função para converter a URL do Render (postgres://) no formato do Npgsql
string GetConnectionString()
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (string.IsNullOrEmpty(databaseUrl))
    {
        return builder.Configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("String de conexão não encontrada.");
    }

    if (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://"))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }

    return databaseUrl;
}

var connectionString = GetConnectionString();

// Configuração do banco de dados PostgreSQL com DbContext (Npgsql)
builder.Services.AddDbContext<ShekinahContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Bloco para aplicar as migrações automaticamente no PostgreSQL da nuvem
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ShekinahContext>();
    context.Database.Migrate();
}

// 3. Configuração do pipeline de requisições HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // UseHttpsRedirection comentado para evitar conflitos com o proxy reverso do Render em produção
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

app.Run();