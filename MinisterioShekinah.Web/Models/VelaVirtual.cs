using System;
using System.ComponentModel.DataAnnotations;

namespace MinisterioShekinah.Web.Models
{
    public class VelaVirtual
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o seu nome.")]
        [StringLength(100)]
        public string NomeFiel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escreva a sua intenção de oração.")]
        [StringLength(500)]
        public string Intencao { get; set; } = string.Empty;

        public string CorVela { get; set; } = "branco";

        public DateTime DataAcesa { get; set; } = DateTime.Now;
        public bool Ativa { get; set; } = true;
    }
}