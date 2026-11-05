using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Data;
using MinisterioShekinah.Web.Hubs;
using MinisterioShekinah.Web.Models;

// Mantém a compatibilidade de datas
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

// Bloco de autoverificação e correção estrita de tipos na nuvem
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ShekinahContext>();

        context.Database.ExecuteSqlRaw(@"
            -- 1. Musicas
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

            CREATE SEQUENCE IF NOT EXISTS ""Musicas_Id_seq"";
            ALTER TABLE ""Musicas"" ALTER COLUMN ""Id"" SET DEFAULT nextval('""Musicas_Id_seq""');

            -- 2. VelasVirtuais
            CREATE TABLE IF NOT EXISTS ""VelasVirtuais"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""Nome"" TEXT,
                ""Intencao"" TEXT,
                ""Data"" TIMESTAMP WITHOUT TIME ZONE,
                ""DataAcesa"" TIMESTAMP WITHOUT TIME ZONE,
                ""Ativa"" BOOLEAN DEFAULT TRUE,
                ""CorVela"" TEXT
            );

            -- CORREÇÃO DE TIPOS: Converte colunas antigas para os tipos corretos na base de dados existente
            ALTER TABLE ""VelasVirtuais"" ALTER COLUMN ""Ativa"" TYPE BOOLEAN USING 
                CASE 
                    WHEN ""Ativa""::text = '0' THEN FALSE 
                    WHEN ""Ativa""::text = '1' THEN TRUE 
                    ELSE COALESCE(""Ativa""::text::boolean, TRUE) 
                END;
            ALTER TABLE ""VelasVirtuais"" ALTER COLUMN ""Ativa"" SET DEFAULT TRUE;

            ALTER TABLE ""VelasVirtuais"" ALTER COLUMN ""DataAcesa"" TYPE TIMESTAMP WITHOUT TIME ZONE USING 
                CASE 
                    WHEN ""DataAcesa"" IS NULL OR ""DataAcesa""::text = '' THEN CURRENT_TIMESTAMP 
                    ELSE ""DataAcesa""::timestamp without time zone 
                END;

            ALTER TABLE ""VelasVirtuais"" ALTER COLUMN ""Data"" TYPE TIMESTAMP WITHOUT TIME ZONE USING 
                CASE 
                    WHEN ""Data"" IS NULL OR ""Data""::text = '' THEN CURRENT_TIMESTAMP 
                    ELSE ""Data""::timestamp without time zone 
                END;
        ");

        Console.WriteLine("--> Tabelas e tipos de dados corrigidos com sucesso na nuvem!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> AVISO NA ATUALIZAÇÃO DO BANCO: {ex.Message}");
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