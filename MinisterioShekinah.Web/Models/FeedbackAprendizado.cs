using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinisterioShekinah.Web.Models
{
    [Table("feedbacksaprendizado")]
    public class FeedbackAprendizado
    {
        public int Id { get; set; }
        public string TipoAcao { get; set; } = string.Empty;
        public string TextoOriginal { get; set; } = string.Empty;
        public string RespostaGeradaPorIa { get; set; } = string.Empty;
        public string CorrecaoDoUsuario { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}