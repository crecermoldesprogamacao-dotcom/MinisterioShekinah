using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class Musica
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        public string Titulo { get; set; } = string.Empty;

        public string? Artista { get; set; }

        [Required(ErrorMessage = "O tom original é obrigatório")]
        public string TomOriginal { get; set; } = "C";

        public int Capotraste { get; set; } = 0; // 0 = Sem capotraste

        [DataType(DataType.MultilineText)]
        public string Cifra { get; set; } = string.Empty;

        public string? PartituraUrl { get; set; } // Link ou caminho do PDF/Imagem da partitura

        // Contexto litúrgico para Missas e Eventos Católicos
        public string? MomentoLiturgico { get; set; } // Ex: "Entrada", "Ato Penitencial", "Salmo", "Ofertório", "Comunhão", etc.
    }
}