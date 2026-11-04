using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class SobreMinisterio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título da história é obrigatório.")]
        [Display(Name = "Título da História")]
        public string TituloHistoria { get; set; } = "A Nossa História";

        [Required(ErrorMessage = "O texto da história é obrigatório.")]
        [Display(Name = "Texto da História")]
        public string TextoHistoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "O título da missão é obrigatório.")]
        [Display(Name = "Título de Missão & Valores")]
        public string TituloMissao { get; set; } = "Missão & Valores";

        [Required(ErrorMessage = "O texto da missão é obrigatório.")]
        [Display(Name = "Texto de Missão & Valores")]
        public string TextoMissao { get; set; } = string.Empty;
    }
}