using Microsoft.EntityFrameworkCore;
using MinisterioShekinah.Web.Models;

namespace MinisterioShekinah.Web.Data
{
    public class ShekinahContext : DbContext
    {
        public ShekinahContext(DbContextOptions<ShekinahContext> options) : base(options)
        {
        }

        public DbSet<Musica> Musicas { get; set; } = null!;
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<EventoMissa> EventosMissas { get; set; }
        // Mapeado com os tipos corretos esperados pelo EventosController:
        public DbSet<MusicaRepertorio> Repertorios { get; set; }
        public DbSet<EscalaMusico> Escalas { get; set; }
        public DbSet<AgendaItem> AgendaItems { get; set; }
        public DbSet<VideoItem> Videos { get; set; }
        public DbSet<MembroEquipe> MembrosEquipa { get; set; }
        public DbSet<SobreMinisterio> SobreMinisterios { get; set; }
        public DbSet<VelaVirtual> VelasVirtuais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignora temporariamente a propriedade MomentoLiturgico para evitar o erro de coluna em falta no PostgreSQL
            modelBuilder.Entity<Musica>().Ignore(m => m.MomentoLiturgico);
        }
    }
}