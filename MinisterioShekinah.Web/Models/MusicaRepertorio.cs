using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class MusicaRepertorio
    {
        public int Id { get; set; }

        public int EventoMissaId { get; set; }
        public virtual EventoMissa? EventoMissa { get; set; }

        [Required(ErrorMessage = "Momento litúrgico obrigatório.")]
        [Display(Name = "Momento Litúrgico")]
        public string Momento { get; set; } = string.Empty; // Ex: Entrada, Comunhão

        [Required(ErrorMessage = "O título da música é obrigatório.")]
        [Display(Name = "Nome da Música")]
        public string TituloMusica { get; set; } = string.Empty;

        [Display(Name = "Tom / Cifra")]
        public string? TomOuCifra { get; set; }
    }
}