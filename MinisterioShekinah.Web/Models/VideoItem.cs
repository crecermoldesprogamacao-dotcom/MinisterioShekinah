using System;
using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class VideoItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do vídeo é obrigatório.")]
        [Display(Name = "Título do Louvor / Vídeo")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A URL do YouTube é obrigatória.")]
        [Display(Name = "URL do YouTube (Embed ou Link)")]
        public string YoutubeUrl { get; set; } = string.Empty;

        [Display(Name = "Descrição / Categoria")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Data de Publicação")]
        public DateTime DataPublicacao { get; set; } = DateTime.Now;
    }
}