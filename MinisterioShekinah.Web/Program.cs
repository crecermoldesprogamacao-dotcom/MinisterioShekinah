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

// Ler a string de conexão do Render com tratamento seguro da porta
string rawDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                        ?? "postgresql://ministerio_shekinah_db_user:M6mFlNXNtA8H6LanRGOH9UsmL94N2EcB@dpg-dbm7d6l6e07s73bcmjq0-a/ministerio_shekinah_db";

string GetConnectionString(string url)
{
    if (url.StartsWith("postgres://") || url.StartsWith("postgresql://"))
    {
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        int dbPort = uri.Port > 0 ? uri.Port : 5432;

        return $"Host={uri.Host};Port={dbPort};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
    return url;
}

var connectionString = GetConnectionString(rawDatabaseUrl);

// Configuração do banco de dados PostgreSQL com DbContext (Npgsql)
builder.Services.AddDbContext<ShekinahContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Bloco para aplicar migrações e garantir que a coluna MomentoLiturgico existe com segurança
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ShekinahContext>();
        context.Database.Migrate();

        // Força a criação da coluna cobrindo tanto o nome com aspas (case-sensitive) como minúsculo
        context.Database.ExecuteSqlRaw(@"
            DO $$ 
            BEGIN 
                -- Tenta adicionar com aspas duplas (compatível com EF Core no PostgreSQL)
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns 
                    WHERE table_name = 'Musicas' AND column_name = 'MomentoLiturgico'
                ) THEN
                    ALTER TABLE ""Musicas"" ADD COLUMN ""MomentoLiturgico"" TEXT;
                END IF;
            END $$;
        ");

        Console.WriteLine("--> Base de dados e coluna MomentoLiturgico verificadas com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> ERRO CRÍTICO NA MIGRAÇÃO: {ex.Message}");
    }
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