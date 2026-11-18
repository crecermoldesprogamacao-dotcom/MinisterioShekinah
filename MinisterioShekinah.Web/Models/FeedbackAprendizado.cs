using System;

namespace MinisterioShekinah.Web.Models
{
    public class FeedbackAprendizado
    {
        public int Id { get; set; }
        public string TipoAcao { get; set; } = string.Empty; // Ex: "OrganizacaoCifra", "SugestaoLiturgia"
        public string TextoOriginal { get; set; } = string.Empty;
        public string RespostaGeradaPorIa { get; set; } = string.Empty;
        public string CorrecaoDoUsuario { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}