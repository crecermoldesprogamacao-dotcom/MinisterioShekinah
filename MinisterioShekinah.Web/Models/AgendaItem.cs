using System;
using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class AgendaItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do evento é obrigatório.")]
        [Display(Name = "Título / Evento")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data e hora são obrigatórias.")]
        [Display(Name = "Data e Hora")]
        public DateTime DataHora { get; set; }

        [Required(ErrorMessage = "O local é obrigatório.")]
        [Display(Name = "Local")]
        public string Local { get; set; } = string.Empty;

        [Display(Name = "Descrição / Observações")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Tipo de Evento")]
        public string Tipo { get; set; } = string.Empty;
    }
}