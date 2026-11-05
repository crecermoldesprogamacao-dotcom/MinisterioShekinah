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

// Bloco de autoverificação e correção definitiva na nuvem
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ShekinahContext>();

        // Executa comandos SQL seguros para alinhar as tabelas e corrigir o auto-incremento do ID
        context.Database.ExecuteSqlRaw(@"
            -- 1. Garante a tabela Musicas e adiciona colunas se faltarem
            CREATE TABLE IF NOT EXISTS ""Musicas"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Titulo"" TEXT,
                ""Artista"" TEXT,
                ""TomOriginal"" TEXT,
                ""Capotraste"" TEXT,
                ""Cifra"" TEXT,
                ""PartituraUrl"" TEXT,
                ""MomentoLiturgico"" TEXT
            );

            DO $$ 
            BEGIN 
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Musicas' AND column_name = 'MomentoLiturgico') THEN
                    ALTER TABLE ""Musicas"" ADD COLUMN ""MomentoLiturgico"" TEXT;
                END IF;
            END $$;

            -- Corrige o ID para garantir o auto-incremento (SERIAL/Identity)
            CREATE SEQUENCE IF NOT EXISTS ""Musicas_Id_seq"";
            ALTER TABLE ""Musicas"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Musicas_Id_seq""');
            ALTER SEQUENCE ""Musicas_Id_seq"" OWNED BY ""Musicas"".""Id"";

            -- 2. Tabela VelasVirtuais
            CREATE TABLE IF NOT EXISTS ""VelasVirtuais"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Nome"" TEXT,
                ""Intencao"" TEXT,
                ""Data"" TIMESTAMP WITHOUT TIME ZONE,
                ""DataAcesa"" TIMESTAMP WITHOUT TIME ZONE,
                ""Ativa"" BOOLEAN DEFAULT TRUE,
                ""CorVela"" TEXT
            );
        ");

        Console.WriteLine("--> Estrutura e auto-incremento corrigidos com sucesso na nuvem!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> AVISO NA CORREÇÃO DO BANCO: {ex.Message}");
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

app.Run();