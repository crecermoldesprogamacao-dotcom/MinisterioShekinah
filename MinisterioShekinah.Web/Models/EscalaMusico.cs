using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class EscalaMusico
    {
        public int Id { get; set; }

        public int EventoMissaId { get; set; }
        public virtual EventoMissa? EventoMissa { get; set; }

        [Required(ErrorMessage = "O nome do músico é obrigatório.")]
        [Display(Name = "Nome do Músico / Servo")]
        public string NomeMusico { get; set; } = string.Empty;

        [Required(ErrorMessage = "O instrumento/função é obrigatório.")]
        [Display(Name = "Instrumento / Função")]
        public string Funcao { get; set; } = string.Empty; // Ex: Vocal, Violão

        [Display(Name = "Confirmado")]
        public bool Confirmado { get; set; } = false; // RSVP
    }
}