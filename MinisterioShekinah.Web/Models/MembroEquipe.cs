using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class MembroEquipe
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        [Display(Name = "Nome do Membro")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A função é obrigatória.")]
        [Display(Name = "Função / Instrumento")]
        public string Funcao { get; set; } = string.Empty;

        [Display(Name = "URL da Foto (Opcional)")]
        public string FotoUrl { get; set; } = string.Empty;

        [Display(Name = "Breve Biografia")]
        public string Bio { get; set; } = string.Empty;
    }
}