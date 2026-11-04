using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class Evento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do evento é obrigatório")]
        [Display(Name = "Título do Evento")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data e Hora")]
        public DateTime DataHora { get; set; }

        [Required(ErrorMessage = "O local é obrigatório")]
        [Display(Name = "Local / Igreja")]
        public string Local { get; set; } = string.Empty;

        [Display(Name = "Link de WhatsApp para Informações")]
        public string LinkWhatsApp { get; set; } = string.Empty;
    }
}