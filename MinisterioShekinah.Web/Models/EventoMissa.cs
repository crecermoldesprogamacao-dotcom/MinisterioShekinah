using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class EventoMissa
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório.")]
        [Display(Name = "Título da Celebração")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data e hora são obrigatórias.")]
        [Display(Name = "Data e Hora")]
        public DateTime DataHora { get; set; } = DateTime.Now.AddDays(1);

        [Required(ErrorMessage = "O local é obrigatório.")]
        [Display(Name = "Local / Igreja")]
        public string Local { get; set; } = string.Empty;

        [Display(Name = "Observações / Cor Litúrgica")]
        public string? Descricao { get; set; }

        public virtual ICollection<MusicaRepertorio> Repertorio { get; set; } = new List<MusicaRepertorio>();
        public virtual ICollection<EscalaMusico> Escala { get; set; } = new List<EscalaMusico>();
    }
}